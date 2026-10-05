using System.Windows;

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
            base.OnStartup(e);
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