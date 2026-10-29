namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// The name of a rank in one language, mapped to the RankLocale table. The game's code
    /// only ever reads this catalog; it never creates or edits a translation.
    /// </summary>
    public class RankLocale
    {
        /// <summary>
        /// Length of the name column.
        /// </summary>
        public const int NameMaxLength = 100;

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization.
        /// </summary>
        protected RankLocale()
        {
        }

        /// <summary>
        /// Gets the identifier of the rank the translation belongs to.
        /// </summary>
        public int RankId { get; private set; }

        /// <summary>
        /// Gets the identifier of the language of the translation.
        /// </summary>
        public int LocaleId { get; private set; }

        /// <summary>
        /// Gets the name of the rank in that language.
        /// </summary>
        public string Name { get; private set; }
    }
}
