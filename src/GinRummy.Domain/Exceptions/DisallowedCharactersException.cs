using System;

namespace GinRummy.Domain
{
    public class DisallowedCharactersException : GameCoreException
    {
        public DisallowedCharactersException()
        {
        }

        public DisallowedCharactersException(string message)
            : base(message)
        {
        }

        public DisallowedCharactersException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
