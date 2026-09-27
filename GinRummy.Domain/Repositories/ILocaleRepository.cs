using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach the Locale catalog, without knowing how
    /// or where it is stored.
    /// </summary>
    public interface ILocaleRepository
    {
        /// <summary>
        /// Finds the locale whose code matches, or null when no locale has it.
        /// </summary>
        /// <param name="localeCode">Culture code to search for, such as "es-MX".</param>
        /// <returns>The matching locale, or null.</returns>
        Locale FindByCode(string localeCode);
    }
}
