using System;

namespace GinRummy.Domain
{
    public class RequiredFieldMissingException : GameCoreException
    {
        public RequiredFieldMissingException()
        {
        }

        public RequiredFieldMissingException(string message)
            : base(message)
        {
        }

        public RequiredFieldMissingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
