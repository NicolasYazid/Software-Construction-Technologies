using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A language and region the game can display, mapped to the Locale table. The
    /// game's code only ever reads this catalog; it never creates or edits a locale.
    /// </summary>
    public class Locale
    {
        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization.
        /// </summary>
        protected Locale()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int LocaleId { get; private set; }

        /// <summary>
        /// Gets the culture code, such as "es-MX" or "en-US".
        /// </summary>
        public string LocaleCode { get; private set; }

        /// <summary>
        /// Gets the name shown to the player for this locale.
        /// </summary>
        public string DisplayName { get; private set; }
    }
}
