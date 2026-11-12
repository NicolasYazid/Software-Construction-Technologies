namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class ProhibitedWordLocale
    {
        protected ProhibitedWordLocale()
        {
        }

        public int ProhibitedWordId { get; private set; }

        public int LocaleId { get; private set; }
    }
}
