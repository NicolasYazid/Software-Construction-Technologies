using System.Linq;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Daos
{
    public class LocaleDao : ILocaleDao
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        public LocaleDao(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        // The context can come from the caller so that several DAOs take part in one transaction.
        // Only the contexts this DAO creates are disposed of here; a shared one belongs to its caller.
        public LocaleDao(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext;
        }

        public Locale FindByCode(string localeCode)
        {
            Locale foundLocale;
            if (_sharedContext != null)
            {
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
