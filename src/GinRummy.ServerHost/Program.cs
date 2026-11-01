using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

using GinRummy.Application.UseCases;
using GinRummy.Domain.Ports.Infrastructure;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Services;
using GinRummy.Infrastructure;
using GinRummy.Persistence.Persistence;
using GinRummy.Persistence.Repositories;
using GinRummy.Security;
using GinRummy.Services.Hosting;
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
        private const string LogFileName = "GinRummy.ServerHost.log";

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
                IEmailSender emailSender = CreateEmailSender(loggerFactory.CreateLogger<Program>());
                ILogger<LogInService> logInLogger = loggerFactory.CreateLogger<LogInService>();

                hostGroup.Add(CreateIsolatedHost(typeof(LeaderboardService), ComposeLeaderboardService, faultShieldBehavior));
                hostGroup.Add(CreateIsolatedHost(
                    typeof(LogInService),
                    () => ComposeLogInService(emailSender, logInLogger),
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

        private static LeaderboardService ComposeLeaderboardService()
        {
            IRankingRepository rankingRepository = new RankingRepository(ConnectionStringName);
            RankResolver rankResolver = new RankResolver();
            ViewLeaderboardUseCase useCase = new ViewLeaderboardUseCase(rankingRepository, rankResolver);
            LeaderboardService leaderboardService = new LeaderboardService(useCase);

            return leaderboardService;
        }

        private static LogInService ComposeLogInService(IEmailSender emailSender, ILogger<LogInService> logger)
        {
            IClock clock = new SystemClock();
            Func<IUnitOfWork> unitOfWorkFactory = () => new EntityFrameworkUnitOfWork(ConnectionStringName);
            AccountAccessPolicy accessPolicy = new AccountAccessPolicy(new Argon2PasswordHasher(), clock);
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
    }
}
