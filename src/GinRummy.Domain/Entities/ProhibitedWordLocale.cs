namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// The link that makes a prohibited word apply to one language, mapped to the
    /// ProhibitedWordLocale table. The game's code only ever reads this catalog; it never
    /// creates or edits a link.
    /// </summary>
    public class ProhibitedWordLocale
    {
        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization.
        /// </summary>
        protected ProhibitedWordLocale()
        {
        }

        /// <summary>
        /// Gets the identifier of the prohibited word.
        /// </summary>
        public int ProhibitedWordId { get; private set; }

        /// <summary>
        /// Gets the identifier of the language the word applies to.
        /// </summary>
        public int LocaleId { get; private set; }
    }
}
