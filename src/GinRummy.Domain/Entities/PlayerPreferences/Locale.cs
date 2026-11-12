namespace GinRummy.Domain.Entities.PlayerPreferences
{
    public class Locale
    {
        public const int LocaleCodeMaxLength = 10;

        public const int DisplayNameMaxLength = 100;

        protected Locale()
        {
        }

        public int LocaleId { get; private set; }

        public string LocaleCode { get; private set; }

        public string DisplayName { get; private set; }

        public bool IsDefault { get; private set; }
    }
}
