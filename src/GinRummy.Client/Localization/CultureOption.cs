namespace GinRummy.Client.Localization
{
    // Both names are written in the language they name, and are therefore never translated.
    public sealed class CultureOption
    {
        private readonly string _code;
        private readonly string _shortName;
        private readonly string _displayName;

        public CultureOption(string code, string shortName, string displayName)
        {
            _code = code;
            _shortName = shortName;
            _displayName = displayName;
        }

        public string Code
        {
            get { return _code; }
        }

        public string ShortName
        {
            get { return _shortName; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public override string ToString()
        {
            return _shortName;
        }
    }
}
