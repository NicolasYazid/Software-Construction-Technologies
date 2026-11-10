using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

using GinRummy.Application.UseCases.AccountProtection;
using GinRummy.Application.UseCases.Authentication;
using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Application.UseCases.PlayerCareer;
using GinRummy.Application.UseCases.PlayerCustomization;
using GinRummy.Application.UseCases.PlayerPresence;
using GinRummy.Application.UseCases.SocialCommunication;
using GinRummy.Contracts.Services.DirectDuel;
using GinRummy.Contracts.Services.Gameplay;
using GinRummy.Contracts.Services.SocialCommunication;
using GinRummy.Domain.Entities.AccountProtection;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Ports.AccountProtection;
using GinRummy.Domain.Ports.Authentication;
using GinRummy.Domain.Ports.PlayerCareer;
using GinRummy.Domain.Shared;
using GinRummy.Infrastructure;
using GinRummy.Persistence.Persistence;
using GinRummy.Persistence.Repositories;
using GinRummy.Security;
using GinRummy.Services.Hosting;
using GinRummy.Services.Notifiers;
using GinRummy.Services.Services;
using Microsoft.Extensions.Logging;

namespace GinRummy.ServerHost
{
    /// <summary>
    /// Entry point and composition root of the server process. Each service gets its own host,
    /// built from the system.serviceModel section of App.config, so a failure in one service
    /// stays in that service.
    /// </summary>
    public class Program
    {
        private const string ConnectionStringName = "GinRummyDb";
        private const string LogFolderName = "Logs";
        private const string ProfilePictureFolderName = "ProfilePictures";
        private const string LogFileName = "GinRummy.ServerHost.log";
        private const string SigningKeyVariableName = "GINRUMMY_TOKEN_SIGNING_KEY";

        /// <summary>
        /// Composes and opens every service and keeps them listening until Enter is pressed.
        /// </summary>
        /// <param name="args">Command line arguments; not used.</param>
        public static void Main(string[] args)
        {
            using (ILoggerFactory loggerFactory = new FileLoggerFactory(BuildLogFilePath(), LogLevel.Information))
            {
                IErrorHandler faultShield = new FaultShieldErrorHandler(loggerFactory.CreateLogger<FaultShieldErrorHandler>());
                IServiceBehavior faultShieldBehavior = new FaultShieldBehavior(faultShield);
                ServiceHostGroup hostGroup = new ServiceHostGroup(loggerFactory.CreateLogger<ServiceHostGroup>());
                ILogger<Program> hostLogger = loggerFactory.CreateLogger<Program>();
                IEmailSender emailSender = CreateEmailSender(hostLogger);
                IAccessTokenSigner tokenSigner = CreateAccessTokenSigner(hostLogger);
                ILogger<LogInService> logInLogger = loggerFactory.CreateLogger<LogInService>();
                ILogger<SignUpService> signUpLogger = loggerFactory.CreateLogger<SignUpService>();
                ILogger<PlayerProfileService> playerProfileLogger = loggerFactory.CreateLogger<PlayerProfileService>();
                ILogger<AccountSettingsService> accountSettingsLogger = loggerFactory.CreateLogger<AccountSettingsService>();

                // One announcer serves every session, because it keeps the connection of every player who is online.
                PlayerPresenceAnnouncer presenceAnnouncer = new PlayerPresenceAnnouncer(loggerFactory.CreateLogger<PlayerConnection>());
                PlayerPresenceOrchestrator presenceOrchestrator = ComposePlayerPresenceOrchestrator(tokenSigner, presenceAnnouncer);
                ILogger<PlayerPresenceService> presenceLogger = loggerFactory.CreateLogger<PlayerPresenceService>();
                OnlinePlayers<IChatMessageCallback> chatOnlinePlayers = new OnlinePlayers<IChatMessageCallback>(
                    loggerFactory.CreateLogger<WcfCallbackDispatcher<IChatMessageCallback>>());
                SendMessageUseCase sendMessageUseCase = ComposeSendMessageUseCase(chatOnlinePlayers);
                AccountAccessPolicy chatAccessPolicy = new AccountAccessPolicy(
                    new Argon2PasswordHasher(),
                    tokenSigner,
                    new SystemClock());

                OnlinePlayers<IChallengeCallback> challengeOnlinePlayers = new OnlinePlayers<IChallengeCallback>(
                    loggerFactory.CreateLogger<WcfCallbackDispatcher<IChallengeCallback>>());
                OnlinePlayers<IGameCallback> gameOnlinePlayers = new OnlinePlayers<IGameCallback>(
                    loggerFactory.CreateLogger<WcfCallbackDispatcher<IGameCallback>>());
                RoundRegistry roundRegistry = new RoundRegistry();
                ChallengeUseCase challengeUseCase = ComposeChallengeUseCase(challengeOnlinePlayers, roundRegistry);
                PlayRoundUseCase playRoundUseCase = new PlayRoundUseCase(roundRegistry, new RoundAnnouncer(gameOnlinePlayers));
                AccountAccessPolicy duelAccessPolicy = new AccountAccessPolicy(
                    new Argon2PasswordHasher(),
                    tokenSigner,
                    new SystemClock());

                hostGroup.Add(CreateIsolatedHost(
                    typeof(ChatMessageService),
                    () => new ChatMessageService(chatAccessPolicy, sendMessageUseCase, chatOnlinePlayers),
                    faultShieldBehavior));
                hostGroup.Add(CreateIsolatedHost(
                    typeof(ChallengeService),
                    () => new ChallengeService(duelAccessPolicy, challengeUseCase, challengeOnlinePlayers),
                    faultShieldBehavior));
                hostGroup.Add(CreateIsolatedHost(
                    typeof(GameService),
                    () => new GameService(duelAccessPolicy, playRoundUseCase, gameOnlinePlayers),
                    faultShieldBehavior));
                hostGroup.Add(CreateIsolatedHost(typeof(LeaderboardService), ComposeLeaderboardService, faultShieldBehavior));
                hostGroup.Add(CreateIsolatedHost(
                    typeof(LogInService),
                    () => ComposeLogInService(emailSender, tokenSigner, logInLogger),
                    faultShieldBehavior));
                hostGroup.Add(CreateIsolatedHost(
                    typeof(PlayerPresenceService),
                    () => new PlayerPresenceService(presenceOrchestrator, presenceAnnouncer, presenceLogger),
                    faultShieldBehavior));
                hostGroup.Add(CreateIsolatedHost(
                    typeof(SignUpService),
                    () => ComposeSignUpService(emailSender, signUpLogger),
                    faultShieldBehavior));
                hostGroup.Add(CreateIsolatedHost(
                    typeof(PlayerProfileService),
                    () => ComposePlayerProfileService(tokenSigner, playerProfileLogger),
                    faultShieldBehavior));
                hostGroup.Add(CreateIsolatedHost(
                    typeof(AccountSettingsService),
                    () => ComposeAccountSettingsService(emailSender, tokenSigner, accountSettingsLogger),
                    faultShieldBehavior));

                hostGroup.OpenAll();

                // The hosts serve requests on their own threads, so the process waits here until someone presses Enter.
                Console.ReadLine();
                hostGroup.CloseAll();
            }
        }

        // The address, binding, timeouts and throttling come from App.config, so the code holds no network values.
        private static ServiceHost CreateIsolatedHost(
            Type serviceType,
            Func<object> createInstance,
            IServiceBehavior faultShieldBehavior)
        {
            ServiceHost host = new ServiceHost(serviceType);
            host.Description.Behaviors.Add(new InstanceFactoryBehavior(createInstance));
            host.Description.Behaviors.Add(faultShieldBehavior);

            return host;
        }

        private static string BuildLogFilePath()
        {
            string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFolderName);

            return Path.Combine(logFolder, LogFileName);
        }

        // The mail settings come from SmtpSettings.config, which stays out of the repository because it holds the sender password.
        // Without that file the server still starts and every email ends as Failed, so the warning tells why no code arrives.
        private static IEmailSender CreateEmailSender(ILogger<Program> logger)
        {
            SmtpSettings smtpSettings = SmtpSettings.FromAppSettings(ConfigurationManager.AppSettings);
            if (!smtpSettings.IsComplete)
            {
                logger.LogWarning("The mail settings are incomplete, so no verification email will be delivered.");
            }

            return new SmtpEmailSender(smtpSettings);
        }

        // The key that signs the access tokens is injected through an environment variable, never written in the code or in a file.
        // Without it, a random key lives only while the process runs, so every token stops working when the server restarts.
        private static IAccessTokenSigner CreateAccessTokenSigner(ILogger<Program> logger)
        {
            byte[] signingKey = ReadSigningKey();
            if (signingKey.Length < HmacAccessTokenSigner.MinimumKeySizeInBytes)
            {
                logger.LogWarning(
                    "The variable {VariableName} does not hold a Base64 key of at least 32 bytes, so a temporary key signs the tokens.",
                    SigningKeyVariableName);
                signingKey = new byte[HmacAccessTokenSigner.MinimumKeySizeInBytes];
                using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
                {
                    randomNumberGenerator.GetBytes(signingKey);
                }
            }

            return new HmacAccessTokenSigner(signingKey);
        }

        // A missing or malformed variable is reported as an empty key, which the caller replaces with a temporary one.
        private static byte[] ReadSigningKey()
        {
            byte[] signingKey = Array.Empty<byte>();
            string encodedKey = Environment.GetEnvironmentVariable(SigningKeyVariableName);
            if (!string.IsNullOrWhiteSpace(encodedKey))
            {
                try
                {
                    signingKey = Convert.FromBase64String(encodedKey.Trim());
                }
                catch (FormatException)
                {
                    signingKey = Array.Empty<byte>();
                }
            }

            return signingKey;
        }

        private static LeaderboardService ComposeLeaderboardService()
        {
            IRankingRepository rankingRepository = new RankingRepository(ConnectionStringName);
            RankResolver rankResolver = new RankResolver();
            ViewLeaderboardUseCase useCase = new ViewLeaderboardUseCase(rankingRepository, rankResolver);
            LeaderboardService leaderboardService = new LeaderboardService(useCase);

            return leaderboardService;
        }

        private static LogInService ComposeLogInService(
            IEmailSender emailSender,
            IAccessTokenSigner tokenSigner,
            ILogger<LogInService> logger)
        {
            IClock clock = new SystemClock();
            Func<IUnitOfWork> unitOfWorkFactory = () => new EntityFrameworkUnitOfWork(ConnectionStringName);
            AccountAccessPolicy accessPolicy = new AccountAccessPolicy(new Argon2PasswordHasher(), tokenSigner, clock);
            VerificationCodeIssuer codeIssuer = new VerificationCodeIssuer(
                new RandomVerificationCodeGenerator(),
                new Sha256VerificationCodeHasher(),
                clock);
            VerificationCodeSender codeSender = new VerificationCodeSender(codeIssuer, emailSender, new VerificationEmailComposer());
            VerificationCodeChecker codeChecker = new VerificationCodeChecker(new Sha256VerificationCodeHasher(), clock);
            VerificationCodeRedeemer codeRedeemer = new VerificationCodeRedeemer(codeChecker, clock);
            LogInUseCase logInUseCase = new LogInUseCase(unitOfWorkFactory, accessPolicy, codeSender);
            VerifyTwoFactorLogInUseCase twoFactorLogInUseCase = new VerifyTwoFactorLogInUseCase(
                unitOfWorkFactory,
                accessPolicy,
                codeRedeemer);
            LogInService logInService = new LogInService(logInUseCase, twoFactorLogInUseCase, logger);

            return logInService;
        }

        private static PlayerPresenceOrchestrator ComposePlayerPresenceOrchestrator(
            IAccessTokenSigner tokenSigner,
            PlayerPresenceAnnouncer announcer)
        {
            IClock clock = new SystemClock();
            Func<IUnitOfWork> unitOfWorkFactory = () => new EntityFrameworkUnitOfWork(ConnectionStringName);
            AccountAccessPolicy accessPolicy = new AccountAccessPolicy(new Argon2PasswordHasher(), tokenSigner, clock);
            PlayerPresenceOrchestrator presenceOrchestrator = new PlayerPresenceOrchestrator(unitOfWorkFactory, accessPolicy, announcer);

            return presenceOrchestrator;
        }

        private static SendMessageUseCase ComposeSendMessageUseCase(OnlinePlayers<IChatMessageCallback> chatOnlinePlayers)
        {
            Func<IUnitOfWork> unitOfWorkFactory = () => new EntityFrameworkUnitOfWork(ConnectionStringName);
            ChatHistory chatHistory = new ChatHistory(new SystemClock());
            ChatMessageAnnouncer chatMessageAnnouncer = new ChatMessageAnnouncer(chatOnlinePlayers);

            return new SendMessageUseCase(unitOfWorkFactory, chatHistory, chatMessageAnnouncer);
        }

        private static ChallengeUseCase ComposeChallengeUseCase(
            OnlinePlayers<IChallengeCallback> challengeOnlinePlayers,
            RoundRegistry roundRegistry)
        {
            Func<IUnitOfWork> unitOfWorkFactory = () => new EntityFrameworkUnitOfWork(ConnectionStringName);
            ChallengeBoard challengeBoard = new ChallengeBoard(roundRegistry);
            ChallengeAnnouncer challengeAnnouncer = new ChallengeAnnouncer(challengeOnlinePlayers);

            return new ChallengeUseCase(unitOfWorkFactory, challengeBoard, challengeAnnouncer);
        }

        private static SignUpService ComposeSignUpService(IEmailSender emailSender, ILogger<SignUpService> logger)
        {
            IClock clock = new SystemClock();
            Func<IUnitOfWork> unitOfWorkFactory = () => new EntityFrameworkUnitOfWork(ConnectionStringName);
            AccountRegistrar accountRegistrar = new AccountRegistrar(new Argon2PasswordHasher(), clock);
            VerificationCodeIssuer codeIssuer = new VerificationCodeIssuer(
                new RandomVerificationCodeGenerator(),
                new Sha256VerificationCodeHasher(),
                clock);
            VerificationCodeSender codeSender = new VerificationCodeSender(codeIssuer, emailSender, new VerificationEmailComposer());
            VerificationCodeChecker codeChecker = new VerificationCodeChecker(new Sha256VerificationCodeHasher(), clock);
            VerificationCodeRedeemer codeRedeemer = new VerificationCodeRedeemer(codeChecker, clock);
            SignUpUseCase signUpUseCase = new SignUpUseCase(unitOfWorkFactory, accountRegistrar, codeSender);
            EmailVerificationUseCase emailVerificationUseCase = new EmailVerificationUseCase(
                unitOfWorkFactory,
                codeRedeemer,
                codeSender);
            SignUpService signUpService = new SignUpService(signUpUseCase, emailVerificationUseCase, logger);

            return signUpService;
        }

        private static PlayerProfileService ComposePlayerProfileService(
            IAccessTokenSigner tokenSigner,
            ILogger<PlayerProfileService> logger)
        {
            Func<IUnitOfWork> unitOfWorkFactory = () => new EntityFrameworkUnitOfWork(ConnectionStringName);
            AccountAccessPolicy accessPolicy = new AccountAccessPolicy(new Argon2PasswordHasher(), tokenSigner, new SystemClock());
            ViewPlayerProfileUseCase viewUseCase = new ViewPlayerProfileUseCase(
                unitOfWorkFactory,
                accessPolicy,
                new RankingRepository(ConnectionStringName));
            string pictureFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ProfilePictureFolderName);
            EditPlayerProfileUseCase editUseCase = new EditPlayerProfileUseCase(
                unitOfWorkFactory,
                accessPolicy,
                new FileProfilePictureStorage(pictureFolderPath));
            PlayerProfileService playerProfileService = new PlayerProfileService(viewUseCase, editUseCase, logger);

            return playerProfileService;
        }

        private static AccountSettingsService ComposeAccountSettingsService(
            IEmailSender emailSender,
            IAccessTokenSigner tokenSigner,
            ILogger<AccountSettingsService> logger)
        {
            IClock clock = new SystemClock();
            Func<IUnitOfWork> unitOfWorkFactory = () => new EntityFrameworkUnitOfWork(ConnectionStringName);
            AccountAccessPolicy accessPolicy = new AccountAccessPolicy(new Argon2PasswordHasher(), tokenSigner, clock);
            VerificationCodeIssuer codeIssuer = new VerificationCodeIssuer(
                new RandomVerificationCodeGenerator(),
                new Sha256VerificationCodeHasher(),
                clock);
            VerificationCodeSender codeSender = new VerificationCodeSender(codeIssuer, emailSender, new VerificationEmailComposer());
            VerificationCodeChecker codeChecker = new VerificationCodeChecker(new Sha256VerificationCodeHasher(), clock);
            VerificationCodeRedeemer codeRedeemer = new VerificationCodeRedeemer(codeChecker, clock);
            AccountSettingsUseCase settingsUseCase = new AccountSettingsUseCase(unitOfWorkFactory, accessPolicy, codeSender);
            ConfirmAccountChangeUseCase confirmationUseCase = new ConfirmAccountChangeUseCase(
                unitOfWorkFactory,
                accessPolicy,
                codeRedeemer);
            AccountSettingsService accountSettingsService = new AccountSettingsService(settingsUseCase, confirmationUseCase, logger);

            return accountSettingsService;
        }
    }
}
