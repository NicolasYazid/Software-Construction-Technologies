namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class ReasonBanLocale
    {
        public const int NameMaxLength = 100;

        protected ReasonBanLocale()
        {
        }

        public int ReasonBanId { get; private set; }

        public int LocaleId { get; private set; }

        public string Name { get; private set; }
    }
}
