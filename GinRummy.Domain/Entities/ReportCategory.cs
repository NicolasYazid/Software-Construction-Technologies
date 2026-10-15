using System.Collections.Generic;
using System.Linq;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A reason a player can pick when reporting another player, mapped to the
    /// ReportCategory table. The game's code only ever reads this catalog; it never creates
    /// or edits a category.
    /// </summary>
    public class ReportCategory
    {
        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization.
        /// </summary>
        protected ReportCategory()
        {
            Translations = new List<ReportCategoryLocale>();
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int ReportCategoryId { get; private set; }

        /// <summary>
        /// Gets the base name of the category, used when a language has no translation.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the names of the category in each language.
        /// </summary>
        public virtual ICollection<ReportCategoryLocale> Translations { get; private set; }

        /// <summary>
        /// Gets the name of the category in the given language.
        /// </summary>
        /// <param name="localeId">Identifier of the language.</param>
        /// <returns>The translated name, or the base name when that language has no translation.</returns>
        public string GetNameFor(int localeId)
        {
            ReportCategoryLocale translation = Translations.FirstOrDefault(candidate => candidate.LocaleId == localeId);
            string name = Name;
            if (translation != null)
            {
                name = translation.Name;
            }

            return name;
        }
    }
}
