using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

using GinRummy.Client.Controllers;
using GinRummy.Client.Localization;
using GinRummy.Data.EntityFramework.Daos;
using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Daos;
using GinRummy.Domain.Security;
using GinRummy.Security;

namespace GinRummy.Client
{
    // Application entry point and composition root: the single place allowed to know the
    // concrete adapters, wiring them to the interfaces the rest of the app depends on.
    public partial class App : Application
    {
        public const string LocalizationResourceKey = "Loc";

        private const string ConnectionStringName = "GinRummyContext";
        private const int RenderingTierShift = 16;
        private const int SoftwareRenderingTier = 0;
        private const double SoftwareBlurScale = 0.3;

        private static readonly string[] BlurredShadowKeys =
        {
            "EfxPanelShadow",
            "EfxTextShadow",
            "EfxSoftShadow",
            "EfxCardShadow"
        };

        private readonly IPasswordHasher _passwordHasher = new Argon2PasswordHasher();
        private readonly IVerificationCodeGenerator _codeGenerator = new RandomVerificationCodeGenerator();
        private readonly IVerificationCodeHasher _codeHasher = new Sha256VerificationCodeHasher();

        public SignUpController CreateSignUpController()
        {
            return new SignUpController(
                CreateUnitOfWork,
                _passwordHasher,
                _codeGenerator,
                _codeHasher);
        }

        public LogInController CreateLogInController()
        {
            return new LogInController(
                new PlayerDao(ConnectionStringName),
                _passwordHasher);
        }

        public RankingsController CreateRankingsController()
        {
            return new RankingsController(new RankingDao(ConnectionStringName));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Resources[LocalizationResourceKey] = LocalizationProvider.Instance;
            LocalizationProvider.Instance.SetCulture(LocalizationProvider.DefaultCultureCode);
            if (IsSoftwareRendering())
            {
                ShortenShadowBlur();
            }

            base.OnStartup(e);
        }

        // WPF reports tier 0 when no graphics hardware is drawing the window. Without it, the
        // blur of every shadow is computed by the processor and grows with its radius, so the
        // radius is shortened on those machines only; the hard-edged raised text needs no blur
        // and keeps its value.
        private static bool IsSoftwareRendering()
        {
            return (RenderCapability.Tier >> RenderingTierShift) == SoftwareRenderingTier;
        }

        // The shared instances are edited in place, before the first window is created, so
        // every element that already points at them by key picks up the shorter blur.
        private void ShortenShadowBlur()
        {
            foreach (string shadowKey in BlurredShadowKeys)
            {
                DropShadowEffect shadow = TryFindResource(shadowKey) as DropShadowEffect;
                if ((shadow != null) && !shadow.IsFrozen)
                {
                    shadow.BlurRadius = shadow.BlurRadius * SoftwareBlurScale;
                }
            }
        }

        // Builds a fresh unit of work each time one is requested. This is the factory the
        // controllers receive as Func<IUnitOfWork>, so each operation gets its own context
        // and transaction.
        private IUnitOfWork CreateUnitOfWork()
        {
            return new EntityFrameworkUnitOfWork(ConnectionStringName);
        }
    }
}