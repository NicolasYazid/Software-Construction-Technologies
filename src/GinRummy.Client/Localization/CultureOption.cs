namespace GinRummy.Client.Localization
{
    /// <summary>
    /// One culture offered to the player in the language selector. The display name is
    /// written in its own language and is therefore never translated.
    /// </summary>
    public sealed class CultureOption
    {
        private readonly string _code;
        private readonly string _displayName;

        /// <summary>
        /// Creates a culture option.
        /// </summary>
        /// <param name="code">Culture code, for example es-MX.</param>
        /// <param name="displayName">Name of the culture written in its own language.</param>
        public CultureOption(string code, string displayName)
        {
            _code = code;
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
        /// Gets the name of the culture written in its own language.
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
        }

        /// <summary>
        /// Returns the display name, which is what the selector shows.
        /// </summary>
        /// <returns>The name of the culture written in its own language.</returns>
        public override string ToString()
        {
            return _displayName;
        }
    }
}
