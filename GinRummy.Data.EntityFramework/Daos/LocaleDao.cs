using System.Linq;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Daos
{
    // Fulfils ILocaleDao using Entity Framework against GinRummy_Dev.
    // By default, it opens a short-lived context per operation.
    // It can also reuse a context opened elsewhere, so it can take part in a shared transaction.
    public class LocaleDao : ILocaleDao
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        public LocaleDao(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        public LocaleDao(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext;
        }

        public Locale FindByCode(string localeCode)
        {
            Locale foundLocale;
            if (_sharedContext != null)
            {
                // The caller owns this context.
                // It is disposed of when the caller's transaction ends.
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
    }
}
