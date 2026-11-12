namespace GinRummy.Domain
{
    public class SelfChallengeException : GameCoreException
    {
        public SelfChallengeException(string message)
            : base(message)
        {
        }
    }
}