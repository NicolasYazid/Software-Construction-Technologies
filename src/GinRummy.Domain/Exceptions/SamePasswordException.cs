using System;

namespace GinRummy.Domain
{
    public class SamePasswordException : GameCoreException
    {
        public SamePasswordException()
        {
        }

        public SamePasswordException(string message)
            : base(message)
        {
        }

        public SamePasswordException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
