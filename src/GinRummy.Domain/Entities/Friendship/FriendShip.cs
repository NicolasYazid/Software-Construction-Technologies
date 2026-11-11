using System;

using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Domain.Entities.Friendship
{
    public class FriendShip
    {
        public FriendShip(int firstPlayerAccountId, int secondPlayerAccountId)
        {
            ValidatePlayers(firstPlayerAccountId, secondPlayerAccountId);

            LowerPlayerAccountId = Math.Min(firstPlayerAccountId, secondPlayerAccountId);
            HigherPlayerAccountId = Math.Max(firstPlayerAccountId, secondPlayerAccountId);
        }

        protected FriendShip()
        {
        }

        public int FriendShipId { get; private set; }
        public int LowerPlayerAccountId { get; private set; }
        public int HigherPlayerAccountId { get; private set; }
        public virtual PlayerAccount LowerPlayerAccount { get; private set; }
        public virtual PlayerAccount HigherPlayerAccount { get; private set; }

        public int GetFriendIdOf(int playerAccountId)
        {
            if ((playerAccountId != LowerPlayerAccountId) && (playerAccountId != HigherPlayerAccountId))
            {
                throw new ArgumentException("The player is not part of this friendship.", nameof(playerAccountId));
            }

            int friendId = LowerPlayerAccountId;
            if (playerAccountId == LowerPlayerAccountId)
            {
                friendId = HigherPlayerAccountId;
            }

            return friendId;
        }

        private static void ValidatePlayers(int firstPlayerAccountId, int secondPlayerAccountId)
        {
            if (firstPlayerAccountId <= 0)
            {
                throw new ArgumentException("First playerAccount id must be positive.", nameof(firstPlayerAccountId));
            }

            if (secondPlayerAccountId <= 0)
            {
                throw new ArgumentException("Second playerAccount id must be positive.", nameof(secondPlayerAccountId));
            }

            if (firstPlayerAccountId == secondPlayerAccountId)
            {
                throw new ArgumentException("A player cannot be a friend of themselves.", nameof(secondPlayerAccountId));
            }
        }
    }
}
