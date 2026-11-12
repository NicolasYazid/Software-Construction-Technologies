using System;

namespace GinRummy.Domain
{
    public class InvalidEmailFormatException : GameCoreException
    {
        public InvalidEmailFormatException()
        {
        }

        public InvalidEmailFormatException(string message)
            : base(message)
        {
        }

        public InvalidEmailFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
