namespace GinRummy.Domain
{
    public class ChallengeNotFoundException : GameCoreException
    {
        public ChallengeNotFoundException(string message)
            : base(message)
        {
        }
    }
}
