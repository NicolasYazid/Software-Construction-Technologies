namespace GinRummy.Domain.Entities.PlayerCareer
{
    public class RankedUsername
    {
        public RankedUsername(string username, Rank rank)
        {
            Username = username;
            Rank = rank;
        }

        public string Username { get; private set; }

        public Rank Rank { get; private set; }
    }
}
