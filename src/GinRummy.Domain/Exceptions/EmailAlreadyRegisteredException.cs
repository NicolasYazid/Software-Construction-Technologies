using System;

namespace GinRummy.Domain
{
    public class EmailAlreadyRegisteredException : GameCoreException
    {
        public EmailAlreadyRegisteredException()
        {
        }

        public EmailAlreadyRegisteredException(string message)
            : base(message)
        {
        }

        public EmailAlreadyRegisteredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
