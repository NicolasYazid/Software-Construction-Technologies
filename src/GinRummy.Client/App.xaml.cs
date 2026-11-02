using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

using GinRummy.Client.LeaderboardServiceReference;
using GinRummy.Client.Localization;
using GinRummy.Client.Logging;
using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;
using GinRummy.Client.Services;
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
        private const string LeaderboardEndpointName = "LeaderboardService";
        private const string LogInEndpointName = "LogInService";
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

        internal void StoreSignedInPlayer(LogInOutcome signedInPlayer)
        {
            SignedInPlayer = signedInPlayer ?? throw new ArgumentNullException(nameof(signedInPlayer));
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
        // On those machines the processor computes every shadow blur, at a cost that grows with its radius.
        // The radius is therefore shortened on those machines only.
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
    }
}
