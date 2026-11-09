using System;

using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Domain.Entities.Friendship
{
    /// <summary>
    /// A friendship between two registered players, mapped to the FriendShip table. The
    /// pair is stored once, with the lower player id as PlayerA, so the same friendship can
    /// never be stored twice in opposite order. Protects its own invariants: both players are
    /// real and they are not the same one.
    /// </summary>
    public class FriendShip
    {
        /// <summary>
        /// Creates a valid new friendship. The order of the arguments does not matter; the
        /// lower id is always stored as PlayerA.
        /// </summary>
        /// <param name="firstPlayerAccountId">Identifier of one friend; must be positive.</param>
        /// <param name="secondPlayerAccountId">Identifier of the other friend; must be positive and different.</param>
        public FriendShip(int firstPlayerAccountId, int secondPlayerAccountId)
        {
            ValidatePlayers(firstPlayerAccountId, secondPlayerAccountId);

            PlayerAId = Math.Min(firstPlayerAccountId, secondPlayerAccountId);
            PlayerBId = Math.Max(firstPlayerAccountId, secondPlayerAccountId);
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected FriendShip()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int FriendShipId { get; private set; }

        /// <summary>
        /// Gets the identifier of the friend with the lower player id.
        /// </summary>
        public int PlayerAId { get; private set; }

        /// <summary>
        /// Gets the identifier of the friend with the higher player id.
        /// </summary>
        public int PlayerBId { get; private set; }

        /// <summary>
        /// Gets the account of the friend with the lower player id, loaded from the
        /// relationship between the two tables.
        /// </summary>
        public virtual PlayerAccount PlayerA { get; private set; }

        /// <summary>
        /// Gets the account of the friend with the higher player id, loaded from the
        /// relationship between the two tables.
        /// </summary>
        public virtual PlayerAccount PlayerB { get; private set; }

        /// <summary>
        /// Gets the identifier of the friend who is not the given player.
        /// </summary>
        /// <param name="playerAccountId">Identifier of one of the two friends.</param>
        /// <returns>The identifier of the other friend.</returns>
        /// <exception cref="ArgumentException">Thrown when the player is not part of the friendship.</exception>
        public int GetFriendIdOf(int playerAccountId)
        {
            if ((playerAccountId != PlayerAId) && (playerAccountId != PlayerBId))
            {
                throw new ArgumentException("The player is not part of this friendship.", nameof(playerAccountId));
            }

            int friendId = PlayerAId;
            if (playerAccountId == PlayerAId)
            {
                friendId = PlayerBId;
            }

            return friendId;
        }

        // Both ids reference accounts that already exist, and nobody befriends themselves.
        private static void ValidatePlayers(int firstPlayerAccountId, int secondPlayerAccountId)
        {
            if (firstPlayerAccountId <= 0)
            {
                throw new ArgumentException("First player id must be positive.", nameof(firstPlayerAccountId));
            }

            if (secondPlayerAccountId <= 0)
            {
                throw new ArgumentException("Second player id must be positive.", nameof(secondPlayerAccountId));
            }

            if (firstPlayerAccountId == secondPlayerAccountId)
            {
                throw new ArgumentException("A player cannot be a friend of themselves.", nameof(secondPlayerAccountId));
            }
        }
    }
}
