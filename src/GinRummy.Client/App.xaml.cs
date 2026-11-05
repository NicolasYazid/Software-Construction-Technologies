using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

using GinRummy.Client.AccountSettingsServiceReference;
using GinRummy.Client.LeaderboardServiceReference;
using GinRummy.Client.Localization;
using GinRummy.Client.Logging;
using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;
using GinRummy.Client.PlayerPresenceServiceReference;
using GinRummy.Client.PlayerProfileServiceReference;
using GinRummy.Client.Services;
using GinRummy.Client.SignUpServiceReference;
using GinRummy.Client.ViewModels;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client
{
    // This composition root is the only place allowed to know the concrete adapters, so the rest of the app depends on interfaces.
    public partial class App : Application
    {
        public const string LocalizationResourceKey = "PrvLocalization";

        private const string FatalErrorTitleKey = "ErrorDialog_LblTitleUnexpected";
        private const string FatalErrorMessageKey = "Error_SysUnexpected";
        private const string AccountSettingsEndpointName = "AccountSettingsService";
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

        private static readonly string[] _blurredShadowKeys =
        {
            "EfxPanelShadow",
            "EfxTextShadow",
            "EfxSoftShadow",
            "EfxFloatShadow",
            "EfxCardShadow"
        };

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<App> _logger;

        // Debug entries are kept while the client is in development because they record why an input was rejected.
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

        // The access token lives only in memory, because the standard forbids writing session identifiers to disk.
        internal LogInOutcome SignedInPlayer { get; private set; }

        // A guest is kept apart from a player, because every screen that needs an account looks only at the player.
        internal LogInOutcome SignedInGuest { get; private set; }

        public RankingsViewModel CreateRankingsViewModel()
        {
            return new RankingsViewModel(
                () => new LeaderboardServiceClient(LeaderboardEndpointName),
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
            return new LobbyChatViewModel(SampleData.GetLobby(), CreatePresenceMonitor(), SignedInPlayer.AccessToken);
        }

        public LobbyChatViewModel CreateGuestLobbyViewModel()
        {
            LobbySnapshot guestLobby = SampleData.GetGuestLobby();
            guestLobby.PlayerName = SignedInGuest.Username;

            return new LobbyChatViewModel(guestLobby, CreatePresenceMonitor(), SignedInGuest.AccessToken);
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

        public GameTableViewModel CreateGameTableViewModel()
        {
            return new GameTableViewModel(SampleData);
        }

        public HouseRulesViewModel CreateHouseRulesViewModel()
        {
            return new HouseRulesViewModel(SampleData);
        }

        public NewPasswordViewModel CreateNewPasswordViewModel()
        {
            return new NewPasswordViewModel(false);
        }

        public NotificationsViewModel CreateNotificationsViewModel()
        {
            return new NotificationsViewModel(SampleData);
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

        public ReconnectingViewModel CreateReconnectingViewModel()
        {
            return new ReconnectingViewModel();
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

        // The language the player chose in an earlier session replaces the one of the menu (CU-20 Post-1).
        internal void StoreSignedInPlayer(LogInOutcome signedInPlayer)
        {
            SignedInPlayer = signedInPlayer ?? throw new ArgumentNullException(nameof(signedInPlayer));
            LocalizationProvider.Instance.SetCulture(signedInPlayer.LocaleCode);
        }

        internal void StoreSignedInGuest(LogInOutcome signedInGuest)
        {
            SignedInGuest = signedInGuest ?? throw new ArgumentNullException(nameof(signedInGuest));
        }

        // Whoever leaves the lobby forgets its token, whether it was a player or a guest.
        internal void ForgetSignedInPlayer()
        {
            SignedInPlayer = null;
            SignedInGuest = null;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
            Resources[LocalizationResourceKey] = LocalizationProvider.Instance;
            LocalizationProvider.Instance.SetCulture(LocalizationProvider.DefaultCultureCode);
            if (IsSoftwareRendering())
            {
                ShortenShadowBlur();
            }

            base.OnStartup(e);
        }

        // An exception that reaches this handler had no catch block, so the state of the screen and of the session is unknown.
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.LogCritical(e.Exception, "An unexpected error closed the client.");
            ShowFatalErrorMessage();
            e.Handled = true;
            Shutdown();
        }

        // The runtime ends the process once this handler returns, so the handler only records the failure and tells the player.
        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
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

        // WPF reports tier 0 when no graphics hardware is drawing the window.
        // On those machines the processor computes every shadow blur at a cost that grows with its radius, so the radius is shortened.
        // The hard-edged raised text shadow has no blur, so it is left out of the shortened keys.
        private static bool IsSoftwareRendering()
        {
            return (RenderCapability.Tier >> RenderingTierShift) == SoftwareRenderingTier;
        }

        // The shared instances are edited in place before any window exists, so every element using them by key gets the shorter blur.
        private void ShortenShadowBlur()
        {
            foreach (string shadowKey in _blurredShadowKeys)
            {
                DropShadowEffect shadow = TryFindResource(shadowKey) as DropShadowEffect;
                if ((shadow != null) && !shadow.IsFrozen)
                {
                    shadow.BlurRadius = shadow.BlurRadius * SoftwareBlurScale;
                }
            }
        }

        // The main menu opens the panel without a session, so its caller has no token and the panel hides the account (CU-20 FA-01).
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

        // The presence monitor is created for every lobby because a monitor whose channel failed cannot be started again.
        private PlayerPresenceMonitor CreatePresenceMonitor()
        {
            return new PlayerPresenceMonitor(
                callbackContext => new PlayerPresenceServiceClient(callbackContext, PresenceEndpointName),
                _loggerFactory.CreateLogger<PlayerPresenceMonitor>());
        }
    }
}
