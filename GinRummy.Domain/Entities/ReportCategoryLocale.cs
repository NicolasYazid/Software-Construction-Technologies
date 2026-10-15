namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// The name of a report category in one language, mapped to the ReportCategoryLocale
    /// table. The game's code only ever reads this catalog; it never creates or edits a
    /// translation.
    /// </summary>
    public class ReportCategoryLocale
    {
        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization.
        /// </summary>
        protected ReportCategoryLocale()
        {
        }

        /// <summary>
        /// Gets the identifier of the report category the translation belongs to.
        /// </summary>
        public int ReportCategoryId { get; private set; }

        /// <summary>
        /// Gets the identifier of the language of the translation.
        /// </summary>
        public int LocaleId { get; private set; }

        /// <summary>
        /// Gets the name of the category in that language.
        /// </summary>
        public string Name { get; private set; }
    }
}
