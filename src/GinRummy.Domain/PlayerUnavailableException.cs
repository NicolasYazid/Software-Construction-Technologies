namespace GinRummy.Domain
{
    public class PlayerUnavailableException : GameCoreException
    {
        public PlayerUnavailableException(string message)
            : base(message)
        {
        }
    }
}
