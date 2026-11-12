namespace GinRummy.Domain
{
    public class OpponentBusyException : GameCoreException
    {
        public OpponentBusyException(string message)
            : base(message)
        {
        }
    }
}