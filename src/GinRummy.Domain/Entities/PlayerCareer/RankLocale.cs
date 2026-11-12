namespace GinRummy.Domain.Entities.PlayerCareer
{
    public class RankLocale
    {
        public const int NameMaxLength = 100;

        protected RankLocale()
        {
        }

        public int RankId { get; private set; }

        public int LocaleId { get; private set; }

        public string Name { get; private set; }
    }
}
