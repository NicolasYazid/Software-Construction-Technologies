using System;

namespace GinRummy.Domain.Entities
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
        /// <param name="firstPlayerId">Identifier of one friend; must be positive.</param>
        /// <param name="secondPlayerId">Identifier of the other friend; must be positive and different.</param>
        public FriendShip(int firstPlayerId, int secondPlayerId)
        {
            ValidatePlayers(firstPlayerId, secondPlayerId);

            PlayerAId = Math.Min(firstPlayerId, secondPlayerId);
            PlayerBId = Math.Max(firstPlayerId, secondPlayerId);
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
        /// <param name="playerId">Identifier of one of the two friends.</param>
        /// <returns>The identifier of the other friend.</returns>
        /// <exception cref="ArgumentException">Thrown when the player is not part of the friendship.</exception>
        public int GetFriendIdOf(int playerId)
        {
            if ((playerId != PlayerAId) && (playerId != PlayerBId))
            {
                throw new ArgumentException("The player is not part of this friendship.", nameof(playerId));
            }

            int friendId = PlayerAId;
            if (playerId == PlayerAId)
            {
                friendId = PlayerBId;
            }

            return friendId;
        }

        // Both ids reference accounts that already exist, and nobody befriends themselves.
        private static void ValidatePlayers(int firstPlayerId, int secondPlayerId)
        {
            if (firstPlayerId <= 0)
            {
                throw new ArgumentException("First player id must be positive.", nameof(firstPlayerId));
            }

            if (secondPlayerId <= 0)
            {
                throw new ArgumentException("Second player id must be positive.", nameof(secondPlayerId));
            }

            if (firstPlayerId == secondPlayerId)
            {
                throw new ArgumentException("A player cannot be a friend of themselves.", nameof(secondPlayerId));
            }
        }
    }
}
