namespace GinRummy.Domain.Entities
{
    // A language and region the game can display, mapped to the Locale table.
    // The game's code only ever reads this catalog and never creates or edits a locale.
    public class Locale
    {
        protected Locale()
        {
        }

        public int LocaleId { get; private set; }
        public string LocaleCode { get; private set; }
        public string DisplayName { get; private set; }
    }
}
