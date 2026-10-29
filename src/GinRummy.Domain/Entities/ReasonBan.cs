using System;
using System.Collections.Generic;
using System.Linq;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A reason a player can be banned and how much of the game the ban restricts, mapped to
    /// the ReasonBan table. The game's code only ever reads this catalog; it never creates or
    /// edits a reason.
    /// </summary>
    public class ReasonBan
    {
        /// <summary>
        /// Length of the name column.
        /// </summary>
        public const int NameMaxLength = 100;

        /// <summary>
        /// Length of the restriction_scope column.
        /// </summary>
        public const int RestrictionScopeNameMaxLength = 20;

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization.
        /// </summary>
        protected ReasonBan()
        {
            Translations = new List<ReasonBanLocale>();
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int ReasonBanId { get; private set; }

        /// <summary>
        /// Gets the base name of the reason, used when a language has no translation.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the restriction scope exactly as the restriction_scope column stores it.
        /// </summary>
        public string RestrictionScopeName { get; private set; }

        /// <summary>
        /// Gets the names of the reason in each language.
        /// </summary>
        public virtual ICollection<ReasonBanLocale> Translations { get; private set; }

        /// <summary>
        /// Gets how much of the game a ban with this reason restricts.
        /// </summary>
        public RestrictionScope RestrictionScope
        {
            get { return (RestrictionScope)Enum.Parse(typeof(RestrictionScope), RestrictionScopeName); }
        }

        /// <summary>
        /// Gets whether a ban with this reason stops the player from writing in the chat. A ban
        /// that restricts the whole account restricts the chat too.
        /// </summary>
        public bool RestrictsChat
        {
            get
            {
                bool isChatOnly = RestrictionScope == RestrictionScope.ChatOnly;
                bool restrictsChat = isChatOnly || RestrictsAccount;

                return restrictsChat;
            }
        }

        /// <summary>
        /// Gets whether a ban with this reason stops the player from signing in and playing.
        /// </summary>
        public bool RestrictsAccount
        {
            get { return RestrictionScope == RestrictionScope.FullAccount; }
        }

        /// <summary>
        /// Gets the name of the reason in the given language.
        /// </summary>
        /// <param name="localeId">Identifier of the language.</param>
        /// <returns>The translated name, or the base name when that language has no translation.</returns>
        public string GetNameFor(int localeId)
        {
            ReasonBanLocale translation = Translations.FirstOrDefault(candidate => candidate.LocaleId == localeId);
            string name = Name;
            if (translation != null)
            {
                name = translation.Name;
            }

            return name;
        }
    }
}
