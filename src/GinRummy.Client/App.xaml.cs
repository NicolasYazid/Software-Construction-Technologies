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
    // This composition root is the only place allowed to know the concrete adapters, so the rest of the app depends on interfaces.
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
            "EfxFloatShadow",
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
            foreach (string shadowKey in BlurredShadowKeys)
            {
                DropShadowEffect shadow = TryFindResource(shadowKey) as DropShadowEffect;
                if ((shadow != null) && !shadow.IsFrozen)
                {
                    shadow.BlurRadius = shadow.BlurRadius * SoftwareBlurScale;
                }
            }
        }

        // The controllers receive a factory instead of a unit of work so each operation gets its own context and transaction.
        private IUnitOfWork CreateUnitOfWork()
        {
            return new EntityFrameworkUnitOfWork(ConnectionStringName);
        }
    }
}