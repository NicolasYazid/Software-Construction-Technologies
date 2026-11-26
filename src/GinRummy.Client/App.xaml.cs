using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

using GinRummy.Client.AccountSettingsServiceReference;
using GinRummy.Client.ChatMessageServiceReference;
using GinRummy.Client.DirectDuelServiceReference;
using GinRummy.Client.FriendRequestResponseServiceReference;
using GinRummy.Client.FriendshipServiceReference;
using GinRummy.Client.GameServiceReference;
using GinRummy.Client.LeaderboardServiceReference;
using GinRummy.Client.Localization;
using GinRummy.Client.Logging;
using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;
using GinRummy.Client.PendingFriendRequestServiceReference;
using GinRummy.Client.PlayerPresenceServiceReference;
using GinRummy.Client.PlayerProfileServiceReference;
using GinRummy.Client.Services;
using GinRummy.Client.SignUpServiceReference;
using GinRummy.Client.ViewModels;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client
{
    public partial class App : Application
    {
        public const string LocalizationResourceKey = "PrvLocalization";

        private const string FatalErrorTitleKey = "ErrorDialog_LblTitleUnexpected";
        private const string FatalErrorMessageKey = "Error_SysUnexpected";
        private const string AccountSettingsEndpointName = "AccountSettingsService";
        private const string ChatEndpointName = "ChatMessageService";
        private const string DirectDuelEndpointName = "DirectDuelService";
        private const string FriendshipEndpointName = "FriendshipService";
        private const string PendingFriendRequestEndpointName = "PendingFriendRequestService";
        private const string FriendRequestResponseEndpointName = "FriendRequestResponseService";
        private const string GameEndpointName = "GameService";
        private const string LeaderboardEndpointName = "LeaderboardService";
        private const string LogInEndpointName = "LogInService";
        private const string PresenceEndpointName = "PlayerPresenceService";
        private const string PlayerProfileEndpointName = "PlayerProfileService";
        private const string SignUpEndpointName = "SignUpService";
        private const string LogFolderName = "GinRummy";
        private const string LogFileName = "client.log";
        private const int RenderingTierShift = 16;
        private const int SoftwareRenderingTier = 0;
        private const double SoftwareBlurScale = 0.3;

        private static readonly string[] s_blurredShadowKeys =
        {
            "EfxPanelShadow",
            "EfxTextShadow",
            "EfxSoftShadow",
            "EfxFloatShadow",
            "EfxCardShadow"
        };

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<App> _logger;

        public App()
        {
            string logFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                LogFolderName,
                LogFileName);
            _loggerFactory = new FileLoggerFactory(logFilePath, LogLevel.Debug);
            _logger = _loggerFactory.CreateLogger<App>();
            SampleData = new SampleDataService();
        }

        internal SampleDataService SampleData { get; private set; }

        internal LogInOutcome SignedInPlayer { get; private set; }

        internal LogInOutcome SignedInGuest { get; private set; }

        public RankingsViewModel CreateRankingsViewModel()
        {
            return new RankingsViewModel(
                () => new LeaderboardServiceClient(LeaderboardEndpointName),
                SignedInPlayer.AccessToken,
                _loggerFactory.CreateLogger<RankingsViewModel>());
        }

        public LogInViewModel CreateLogInViewModel()
        {
            return new LogInViewModel(
                () => new LogInServiceClient(LogInEndpointName),
                _loggerFactory.CreateLogger<LogInViewModel>());
        }

        public TwoStepLogInViewModel CreateTwoStepLogInViewModel(LogInCredentials credentials)
        {
            return new TwoStepLogInViewModel(
                credentials,
                () => new LogInServiceClient(LogInEndpointName),
                _loggerFactory.CreateLogger<TwoStepLogInViewModel>());
        }

        public LobbyChatViewModel CreateLobbyChatViewModel()
        {
            return CreateLobbyChatViewModel(SignedInPlayer);
        }

        public LobbyChatViewModel CreateGuestLobbyViewModel()
        {
            return CreateLobbyChatViewModel(SignedInGuest);
        }

        public SignUpViewModel CreateSignUpViewModel(bool isGuestSessionActive)
        {
            return new SignUpViewModel(
                () => new SignUpServiceClient(SignUpEndpointName),
                _loggerFactory.CreateLogger<SignUpViewModel>(),
                isGuestSessionActive);
        }

        public SignUpVerifyEmailViewModel CreateSignUpVerifyEmailViewModel(string email)
        {
            return new SignUpVerifyEmailViewModel(
                email,
                () => new SignUpServiceClient(SignUpEndpointName),
                _loggerFactory.CreateLogger<SignUpVerifyEmailViewModel>());
        }

        public AccountBannedViewModel CreateAccountBannedViewModel(string reasonName)
        {
            return new AccountBannedViewModel(reasonName);
        }

        public AccountSuspendedViewModel CreateAccountSuspendedViewModel(string reasonName, TimeSpan remainingTime)
        {
            return new AccountSuspendedViewModel(reasonName, remainingTime);
        }

        public ConfirmDialogViewModel CreateConfirmDialogViewModel(ConfirmDialogKind kind)
        {
            return new ConfirmDialogViewModel(kind);
        }

        public EditProfileViewModel CreateEditProfileViewModel(PlayerProfile profile)
        {
            return new EditProfileViewModel(
                profile,
                () => new PlayerProfileServiceClient(PlayerProfileEndpointName),
                _loggerFactory.CreateLogger<EditProfileViewModel>());
        }

        public ErrorDialogViewModel CreateErrorDialogViewModel(ErrorDialogKind kind)
        {
            return new ErrorDialogViewModel(kind);
        }

        public LobbyChallengeViewModel CreateLobbyChallengeViewModel()
        {
            DirectDuelConnection duelConnection = new DirectDuelConnection(
                callbackContext => new DirectDuelServiceClient(callbackContext, DirectDuelEndpointName),
                _loggerFactory.CreateLogger<DirectDuelConnection>());
            GameConnection gameConnection = new GameConnection(
                callbackContext => new GameServiceClient(callbackContext, GameEndpointName),
                _loggerFactory.CreateLogger<GameConnection>());

            return new LobbyChallengeViewModel(duelConnection, gameConnection, SignedInPlayer);
        }

        public FriendshipConnection CreateFriendshipConnection()
        {
            return new FriendshipConnection(
                callbackContext => new FriendshipServiceClient(callbackContext, FriendshipEndpointName),
                _loggerFactory.CreateLogger<FriendshipConnection>());
        }

        public LobbyFriendshipViewModel CreateLobbyFriendshipViewModel(FriendshipConnection friendshipConnection)
        {
            return new LobbyFriendshipViewModel(friendshipConnection, SignedInPlayer);
        }

        public HouseRulesViewModel CreateHouseRulesViewModel()
        {
            return new HouseRulesViewModel(SampleData);
        }

        public NewPasswordViewModel CreateNewPasswordViewModel()
        {
            return new NewPasswordViewModel(false);
        }

        public NotificationsViewModel CreateNotificationsViewModel(FriendshipConnection friendshipConnection)
        {
            PendingFriendRequestsConnection pendingRequestsConnection = new PendingFriendRequestsConnection(
                () => new PendingFriendRequestServiceClient(PendingFriendRequestEndpointName),
                SignedInPlayer.AccessToken,
                _loggerFactory.CreateLogger<PendingFriendRequestsConnection>());
            FriendRequestResponseConnection responseConnection = new FriendRequestResponseConnection(
                () => new FriendRequestResponseServiceClient(FriendRequestResponseEndpointName),
                SignedInPlayer.AccessToken,
                _loggerFactory.CreateLogger<FriendRequestResponseConnection>());

            return new NotificationsViewModel(friendshipConnection, pendingRequestsConnection, responseConnection);
        }

        public PlayerProfileViewModel CreatePlayerProfileViewModel(PlayerProfile profile)
        {
            return new PlayerProfileViewModel(
                profile,
                () => new PlayerProfileServiceClient(PlayerProfileEndpointName),
                _loggerFactory.CreateLogger<PlayerProfileViewModel>());
        }

        public ProfilePanelViewModel CreateProfilePanelViewModel()
        {
            return new ProfilePanelViewModel(
                () => new PlayerProfileServiceClient(PlayerProfileEndpointName),
                CreateAccountSettingsCaller(),
                _loggerFactory.CreateLogger<ProfilePanelViewModel>());
        }

        public PasswordChangeViewModel CreatePasswordChangeViewModel()
        {
            return new PasswordChangeViewModel(CreateAccountSettingsCaller());
        }

        public TwoStepActivationViewModel CreateTwoStepActivationViewModel()
        {
            return new TwoStepActivationViewModel(CreateAccountSettingsCaller());
        }

        public TwoStepPasswordChangeViewModel CreateTwoStepPasswordChangeViewModel(string currentPassword, string newPassword)
        {
            return new TwoStepPasswordChangeViewModel(CreateAccountSettingsCaller(), currentPassword, newPassword);
        }

        public EmailChangeVerifyEmailViewModel CreateEmailChangeVerifyEmailViewModel(string newEmail)
        {
            return new EmailChangeVerifyEmailViewModel(CreateAccountSettingsCaller(), newEmail);
        }

        public ReconnectingViewModel CreateReconnectingViewModel(Func<bool> tryReconnect)
        {
            return new ReconnectingViewModel(tryReconnect);
        }

        public RecoverPasswordViewModel CreateRecoverPasswordViewModel()
        {
            return new RecoverPasswordViewModel();
        }

        public SanctionsViewModel CreateSanctionsViewModel()
        {
            return new SanctionsViewModel(SampleData);
        }

        public VerifyEmailViewModel CreateVerifyEmailViewModel(VerificationPurpose purpose, string destinationAddress)
        {
            return new VerifyEmailViewModel(purpose, destinationAddress);
        }

        internal void StoreSignedInPlayer(LogInOutcome signedInPlayer)
        {
            SignedInPlayer = signedInPlayer ?? throw new ArgumentNullException(nameof(signedInPlayer));
            LocalizationProvider.Instance.SetCulture(signedInPlayer.LocaleCode);
        }

        internal void StoreSignedInGuest(LogInOutcome signedInGuest)
        {
            SignedInGuest = signedInGuest ?? throw new ArgumentNullException(nameof(signedInGuest));
        }

        internal void ForgetSignedInPlayer()
        {
            SignedInPlayer = null;
            SignedInGuest = null;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += HandleDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += HandleDomainUnhandledException;
            Resources[LocalizationResourceKey] = LocalizationProvider.Instance;
            LocalizationProvider.Instance.SetCulture(LocalizationProvider.DefaultCultureCode);
            if (IsSoftwareRendering())
            {
                ShortenShadowBlur();
            }

            base.OnStartup(e);
        }

        private void HandleDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.LogCritical(e.Exception, "An unexpected error closed the client.");
            ShowFatalErrorMessage();
            e.Handled = true;
            Shutdown();
        }

        private void HandleDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.LogCritical(e.ExceptionObject as Exception, "An unexpected error outside the interface thread closed the client.");
            ShowFatalErrorMessage();
        }

        private static void ShowFatalErrorMessage()
        {
            LocalizationProvider localization = LocalizationProvider.Instance;
            MessageBox.Show(
                localization.GetText(FatalErrorMessageKey),
                localization.GetText(FatalErrorTitleKey),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private static bool IsSoftwareRendering()
        {
            return (RenderCapability.Tier >> RenderingTierShift) == SoftwareRenderingTier ? true : false;
        }

        private void ShortenShadowBlur()
        {
            foreach (string shadowKey in s_blurredShadowKeys)
            {
                DropShadowEffect shadow = TryFindResource(shadowKey) as DropShadowEffect;
                if ((shadow != null) && !shadow.IsFrozen)
                {
                    shadow.BlurRadius = shadow.BlurRadius * SoftwareBlurScale;
                }
            }
        }

        private AccountSettingsCaller CreateAccountSettingsCaller()
        {
            string accessToken = null;
            if (SignedInPlayer != null)
            {
                accessToken = SignedInPlayer.AccessToken;
            }

            return new AccountSettingsCaller(
                accessToken,
                () => new AccountSettingsServiceClient(AccountSettingsEndpointName),
                _loggerFactory.CreateLogger<AccountSettingsCaller>());
        }

        private LobbyChatViewModel CreateLobbyChatViewModel(LogInOutcome signedInPlayer)
        {
            PlayerPresenceMonitor presenceMonitor = new PlayerPresenceMonitor(
                callbackContext => new PlayerPresenceServiceClient(callbackContext, PresenceEndpointName),
                _loggerFactory.CreateLogger<PlayerPresenceMonitor>());
            ChatMessageConnection chatConnection = new ChatMessageConnection(
                callbackContext => new ChatMessageServiceClient(callbackContext, ChatEndpointName),
                _loggerFactory.CreateLogger<ChatMessageConnection>());

            return new LobbyChatViewModel(presenceMonitor, chatConnection, signedInPlayer);
        }
    }
}
