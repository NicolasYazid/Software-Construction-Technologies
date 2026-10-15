using System;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A pending friendship offer from one registered player to another, mapped to the
    /// FriendRequest table. Accepting or declining it deletes it. Protects its own
    /// invariants: both players are real and they are not the same one.
    /// </summary>
    public class FriendRequest
    {
        /// <summary>
        /// Creates a valid new friend request.
        /// </summary>
        /// <param name="requesterPlayerId">Identifier of the player who sends the request; must be positive.</param>
        /// <param name="recipientPlayerId">Identifier of the player who receives it; must be positive and different.</param>
        /// <param name="createdAt">Moment the request was sent, in UTC.</param>
        public FriendRequest(int requesterPlayerId, int recipientPlayerId, DateTime createdAt)
        {
            ValidatePlayers(requesterPlayerId, recipientPlayerId);

            RequesterPlayerId = requesterPlayerId;
            RecipientPlayerId = recipientPlayerId;
            CreatedAt = createdAt;
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected FriendRequest()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int FriendRequestId { get; private set; }

        /// <summary>
        /// Gets the identifier of the player who sent the request.
        /// </summary>
        public int RequesterPlayerId { get; private set; }

        /// <summary>
        /// Gets the identifier of the player who received the request.
        /// </summary>
        public int RecipientPlayerId { get; private set; }

        /// <summary>
        /// Gets the moment the request was sent, in UTC.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the account of the player who sent the request, loaded from the relationship
        /// between the two tables.
        /// </summary>
        public virtual PlayerAccount Requester { get; private set; }

        /// <summary>
        /// Gets the account of the player who received the request, loaded from the
        /// relationship between the two tables.
        /// </summary>
        public virtual PlayerAccount Recipient { get; private set; }

        // Both ids reference accounts that already exist, and nobody befriends themselves.
        private static void ValidatePlayers(int requesterPlayerId, int recipientPlayerId)
        {
            if (requesterPlayerId <= 0)
            {
                throw new ArgumentException("Requester id must be positive.", nameof(requesterPlayerId));
            }

            if (recipientPlayerId <= 0)
            {
                throw new ArgumentException("Recipient id must be positive.", nameof(recipientPlayerId));
            }

            if (requesterPlayerId == recipientPlayerId)
            {
                throw new ArgumentException("A player cannot send a friend request to themselves.", nameof(recipientPlayerId));
            }
        }
    }
}
