namespace GinRummy.Domain.Entities
{
    public class Rank
    {
        protected Rank()
        {
        }

        public int RankId { get; private set; }
        public string Name { get; private set; }
        public int MinimumScore { get; private set; }
        public int MaximumScore { get; private set; }
    }
}
