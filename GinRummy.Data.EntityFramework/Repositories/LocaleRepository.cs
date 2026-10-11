using System.Linq;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;

namespace GinRummy.Data.EntityFramework.Repositories
{
    public class LocaleRepository : ILocaleRepository
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        public LocaleRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        // The context can come from the caller so that several repositories take part in one transaction.
        // Only the contexts this repository creates are disposed of here; a shared one belongs to its caller.
        public LocaleRepository(GinRummyContext sharedContext)
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
