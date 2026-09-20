namespace GinRummy.Client.Localization
{
    /// <summary>
    /// One culture offered to the player in the language selector. Both names are written in
    /// the language they name, and are therefore never translated.
    /// </summary>
    public sealed class CultureOption
    {
        private readonly string _code;
        private readonly string _shortName;
        private readonly string _displayName;

        /// <summary>
        /// Creates a culture option.
        /// </summary>
        /// <param name="code">Culture code, for example es-MX.</param>
        /// <param name="shortName">Short name shown in the selector of the main menu.</param>
        /// <param name="displayName">Full name, used where there is room for it.</param>
        public CultureOption(string code, string shortName, string displayName)
        {
            _code = code;
            _shortName = shortName;
            _displayName = displayName;
        }

        /// <summary>
        /// Gets the culture code, for example es-MX.
        /// </summary>
        public string Code
        {
            get { return _code; }
        }

        /// <summary>
        /// Gets the short name shown in the selector, as the prototype draws it.
        /// </summary>
        public string ShortName
        {
            get { return _shortName; }
        }

        /// <summary>
        /// Gets the full name of the culture written in its own language.
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
        }

        /// <summary>
        /// Returns the short name, which is what the selector shows.
        /// </summary>
        /// <returns>The short name of the culture.</returns>
        public override string ToString()
        {
            return _shortName;
        }
    }
}
