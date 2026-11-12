using System;

namespace GinRummy.Domain
{
    public class EmailAlreadyVerifiedException : GameCoreException
    {
        public EmailAlreadyVerifiedException()
        {
        }

        public EmailAlreadyVerifiedException(string message)
            : base(message)
        {
        }

        public EmailAlreadyVerifiedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
