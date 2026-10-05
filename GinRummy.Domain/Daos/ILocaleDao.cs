using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Daos
{
    // Contract the game's logic uses to reach the Locale catalog, without knowing how or where
    // it is stored.
    public interface ILocaleDao
    {
        Locale FindByCode(string localeCode);
    }
}
