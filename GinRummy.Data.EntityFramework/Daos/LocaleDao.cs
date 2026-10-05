using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Daos
{
    // Fulfils ILocaleDao using Entity Framework against GinRummy_Dev. By default, opens a
    // short-lived context per operation; optionally reuses a context someone else already
    // opened, so it can take part in a shared transaction.
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
    }
}
