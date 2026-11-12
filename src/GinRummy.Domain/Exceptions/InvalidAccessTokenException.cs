using System;

namespace GinRummy.Domain
{
    public class InvalidAccessTokenException : GameCoreException
    {
        public InvalidAccessTokenException()
        {
        }

        public InvalidAccessTokenException(string message)
            : base(message)
        {
        }

        public InvalidAccessTokenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
