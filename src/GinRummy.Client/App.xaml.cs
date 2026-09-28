using System.Windows;

using GinRummy.Client.Controllers;
using GinRummy.Client.Localization;
using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Data.EntityFramework.Repositories;
using GinRummy.Domain.Repositories;
using GinRummy.Domain.Security;
using GinRummy.Security;

namespace GinRummy.Client
{
    /// <summary>
    /// Application entry point and composition root: the single place allowed to know the
    /// concrete adapters, wiring them to the interfaces the rest of the app depends on.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Key under which the localization provider is published. Every binding to
        /// visible text uses it as its source.
        /// </summary>
        public const string LocalizationResourceKey = "Loc";

        private const string ConnectionStringName = "GinRummyContext";

        private readonly IPasswordHasher _passwordHasher = new Argon2PasswordHasher();
        private readonly IVerificationCodeGenerator _codeGenerator = new RandomVerificationCodeGenerator();
        private readonly IVerificationCodeHasher _codeHasher = new Sha256VerificationCodeHasher();

        /// <summary>
        /// Builds a sign-up controller wired to real, concrete adapters. The screens call
        /// this instead of constructing anything concrete themselves.
        /// </summary>
        /// <returns>A ready-to-use sign-up controller.</returns>
        public SignUpController CreateSignUpController()
        {
            return new SignUpController(
                CreateUnitOfWork,
                _passwordHasher,
                _codeGenerator,
                _codeHasher);
        }

        /// <summary>
        /// Builds a sign-in controller wired to real, concrete adapters. The screens call
        /// this instead of constructing anything concrete themselves.
        /// </summary>
        /// <returns>A ready-to-use sign-in controller.</returns>
        public LogInController CreateLogInController()
        {
            return new LogInController(
                new PlayerRepository(ConnectionStringName),
                _passwordHasher);
        }

        /// <summary>
        /// Builds a rankings controller wired to real, concrete adapters. The screens call
        /// this instead of constructing anything concrete themselves.
        /// </summary>
        /// <returns>A ready-to-use rankings controller.</returns>
        public RankingsController CreateRankingsController()
        {
            return new RankingsController(new RankingRepository(ConnectionStringName));
        }

        /// <summary>
        /// Publishes the localization provider before the first window is loaded.
        /// </summary>
        /// <param name="e">Startup arguments supplied by the framework.</param>
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