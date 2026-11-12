using System;

namespace GinRummy.Domain
{
    public class InvalidCredentialsException : GameCoreException
    {
        public InvalidCredentialsException()
        {
        }

        public InvalidCredentialsException(string message)
            : base(message)
        {
        }

        public InvalidCredentialsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
