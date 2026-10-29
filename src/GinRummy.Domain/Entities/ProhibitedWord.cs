using System.Collections.Generic;
using System.Linq;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A term the chat filter looks for, mapped to the ProhibitedWord table. The languages it
    /// applies to are listed in its locales. The game's code only ever reads this catalog; it
    /// never creates or edits a word.
    /// </summary>
    public class ProhibitedWord
    {
        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization.
        /// </summary>
        protected ProhibitedWord()
        {
            Locales = new List<ProhibitedWordLocale>();
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int ProhibitedWordId { get; private set; }

        /// <summary>
        /// Gets the term the filter looks for.
        /// </summary>
        public string Term { get; private set; }

        /// <summary>
        /// Gets the links to the languages the term applies to.
        /// </summary>
        public virtual ICollection<ProhibitedWordLocale> Locales { get; private set; }

        /// <summary>
        /// Tells whether the term applies to the given language.
        /// </summary>
        /// <param name="localeId">Identifier of the language.</param>
        /// <returns>True when the term is prohibited in that language.</returns>
        public bool AppliesTo(int localeId)
        {
            bool appliesToLocale = Locales.Any(link => link.LocaleId == localeId);

            return appliesToLocale;
        }
    }
}
