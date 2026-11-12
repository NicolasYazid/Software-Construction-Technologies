using System;

namespace GinRummy.Domain
{
    public class VerificationEmailNotSentException : GameCoreException
    {
        public VerificationEmailNotSentException()
        {
        }

        public VerificationEmailNotSentException(string message)
            : base(message)
        {
        }

        public VerificationEmailNotSentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
