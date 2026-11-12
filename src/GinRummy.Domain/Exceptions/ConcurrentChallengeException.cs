namespace GinRummy.Domain
{
    public class ConcurrentChallengeException : GameCoreException
    {
        public ConcurrentChallengeException(string message)
            : base(message)
        {
        }
    }
}