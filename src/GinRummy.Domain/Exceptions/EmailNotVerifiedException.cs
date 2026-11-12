using System;

namespace GinRummy.Domain
{
    public class EmailNotVerifiedException : GameCoreException
    {
        public EmailNotVerifiedException()
        {
        }

        public EmailNotVerifiedException(string message)
            : base(message)
        {
        }

        public EmailNotVerifiedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
