using System.Collections.Generic;
using System.Linq;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    /// <summary>
    /// Fulfils ILocaleRepository using Entity Framework against GinRummy_Dev. By default,
    /// opens a short-lived context per operation; optionally reuses a context someone else
    /// already opened, so it can take part in a shared transaction.
    /// </summary>
    public class LocaleRepository : ILocaleRepository
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        /// <summary>
        /// Builds the repository so that each operation opens and closes its own context.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        public LocaleRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        /// <summary>
        /// Builds the repository so that every operation runs on a context someone else
        /// already opened, so it can share a transaction with other repositories. The
        /// caller stays responsible for disposing that context.
        /// </summary>
        /// <param name="sharedContext">Context to reuse instead of opening a new one.</param>
        public LocaleRepository(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext;
        }

        /// <summary>
        /// Finds the locale whose code matches, or null when no locale has it.
        /// </summary>
        /// <param name="localeCode">Culture code to search for, such as "es-MX".</param>
        /// <returns>The matching locale, or null.</returns>
        public Locale FindByCode(string localeCode)
        {
            Locale foundLocale;
            if (_sharedContext != null)
            {
                // Not wrapped in using: the caller opened this context and stays
                // responsible for closing it.
                foundLocale = _sharedContext.Locales.FirstOrDefault(locale => locale.LocaleCode == localeCode);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundLocale = context.Locales.FirstOrDefault(locale => locale.LocaleCode == localeCode);
                }
            }

            return foundLocale;
        }

        /// <summary>
        /// Finds the locale whose identifier matches, or null when no locale has it.
        /// </summary>
        /// <param name="localeId">Identifier of the locale.</param>
        /// <returns>The matching locale, or null.</returns>
        public Locale FindById(int localeId)
        {
            Locale foundLocale;
            if (_sharedContext != null)
            {
                foundLocale = _sharedContext.Locales.FirstOrDefault(locale => locale.LocaleId == localeId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    foundLocale = context.Locales.FirstOrDefault(locale => locale.LocaleId == localeId);
                }
            }

            return foundLocale;
        }

        /// <summary>
        /// Gets every locale the game supports.
        /// </summary>
        /// <returns>The locales, ordered by identifier.</returns>
        public IList<Locale> GetAll()
        {
            IList<Locale> locales;
            if (_sharedContext != null)
            {
                locales = _sharedContext.Locales.OrderBy(locale => locale.LocaleId).ToList();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    locales = context.Locales.OrderBy(locale => locale.LocaleId).ToList();
                }
            }

            return locales;
        }
    }
}
