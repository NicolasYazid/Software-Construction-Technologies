using System;

namespace GinRummy.Domain
{
    public class FieldTooLongException : GameCoreException
    {
        public FieldTooLongException()
        {
        }

        public FieldTooLongException(string message)
            : base(message)
        {
        }

        public FieldTooLongException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
