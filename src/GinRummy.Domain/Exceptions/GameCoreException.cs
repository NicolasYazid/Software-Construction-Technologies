using System;

namespace GinRummy.Domain
{
    public class GameCoreException : Exception
    {
        public GameCoreException()
        {
        }

        public GameCoreException(string message)
            : base(message)
        {
        }

        public GameCoreException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
