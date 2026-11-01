namespace GinRummy.Domain.Entities.PlayerModeration
{
    /// <summary>
    /// The name of a ban reason in one language, mapped to the ReasonBanLocale table. The
    /// game's code only ever reads this catalog; it never creates or edits a translation.
    /// </summary>
    public class ReasonBanLocale
    {
        /// <summary>
        /// Length of the name column.
        /// </summary>
        public const int NameMaxLength = 100;

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization.
        /// </summary>
        protected ReasonBanLocale()
        {
        }

        /// <summary>
        /// Gets the identifier of the ban reason the translation belongs to.
        /// </summary>
        public int ReasonBanId { get; private set; }

        /// <summary>
        /// Gets the identifier of the language of the translation.
        /// </summary>
        public int LocaleId { get; private set; }

        /// <summary>
        /// Gets the name of the reason in that language.
        /// </summary>
        public string Name { get; private set; }
    }
}
