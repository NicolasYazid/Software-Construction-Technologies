using System.Collections.Generic;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Ports.Repositories
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

        /// <summary>
        /// Finds the locale whose identifier matches, or null when no locale has it.
        /// </summary>
        /// <param name="localeId">Identifier of the locale.</param>
        /// <returns>The matching locale, or null.</returns>
        Locale FindById(int localeId);

        /// <summary>
        /// Gets every locale the game supports.
        /// </summary>
        /// <returns>The locales, ordered by identifier.</returns>
        IList<Locale> GetAll();
    }
}
