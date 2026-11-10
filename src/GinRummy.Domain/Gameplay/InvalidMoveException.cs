using System;

namespace GinRummy.Domain.Gameplay
{
    public class InvalidMoveException : GameCoreException
    {
        public InvalidMoveException()
        {
        }

        public InvalidMoveException(string message)
            : base(message)
        {
        }

        public InvalidMoveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
