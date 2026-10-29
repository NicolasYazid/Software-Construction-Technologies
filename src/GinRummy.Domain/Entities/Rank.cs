using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A rank tier, mapped to the Rank table. A player's rank is the tier whose score
    /// range contains the player's score.
    /// </summary>
    public class Rank
    {
        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization.
        /// </summary>
        protected Rank()
        {
            Translations = new List<RankLocale>();
        }

        /// <summary>
        /// Gets the internal identifier.
        /// </summary>
        public int RankId { get; private set; }

        /// <summary>
        /// Gets the display name of the rank, such as "S" or "A".
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the lowest score that belongs to this rank.
        /// </summary>
        public int MinimumScore { get; private set; }

        /// <summary>
        /// Gets the highest score that belongs to this rank.
        /// </summary>
        public int MaximumScore { get; private set; }

        /// <summary>
        /// Gets the names of the rank in each language.
        /// </summary>
        public virtual ICollection<RankLocale> Translations { get; private set; }

        /// <summary>
        /// Gets the name of the rank in the given language.
        /// </summary>
        /// <param name="localeId">Identifier of the language.</param>
        /// <returns>The translated name, or the base name when that language has no translation.</returns>
        public string GetNameFor(int localeId)
        {
            RankLocale translation = Translations.FirstOrDefault(candidate => candidate.LocaleId == localeId);
            string name = Name;
            if (translation != null)
            {
                name = translation.Name;
            }

            return name;
        }
    }
}
