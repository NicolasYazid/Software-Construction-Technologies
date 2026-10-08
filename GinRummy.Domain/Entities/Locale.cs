namespace GinRummy.Domain.Entities
{
    // The game only reads the locale catalog, so this entity has no public constructor or setters.
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
