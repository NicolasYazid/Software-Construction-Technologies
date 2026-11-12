using System;

namespace GinRummy.Domain
{
    public class WeakPasswordException : GameCoreException
    {
        public WeakPasswordException()
        {
        }

        public WeakPasswordException(string message)
            : base(message)
        {
        }

        public WeakPasswordException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
