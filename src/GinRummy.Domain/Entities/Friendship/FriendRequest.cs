using System;

using GinRummy.Domain.Entities.Players;

namespace GinRummy.Domain.Entities.Friendship
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
        /// <param name="requesterPlayerAccountId">Identifier of the player who sends the request; must be positive.</param>
        /// <param name="recipientPlayerAccountId">Identifier of the player who receives it; must be positive and different.</param>
        /// <param name="createdAt">Moment the request was sent, in UTC.</param>
        public FriendRequest(int requesterPlayerAccountId, int recipientPlayerAccountId, DateTime createdAt)
        {
            ValidatePlayers(requesterPlayerAccountId, recipientPlayerAccountId);

            RequesterPlayerAccountId = requesterPlayerAccountId;
            RecipientPlayerAccountId = recipientPlayerAccountId;
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
        public int RequesterPlayerAccountId { get; private set; }

        /// <summary>
        /// Gets the identifier of the player who received the request.
        /// </summary>
        public int RecipientPlayerAccountId { get; private set; }

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
        private static void ValidatePlayers(int requesterPlayerAccountId, int recipientPlayerAccountId)
        {
            if (requesterPlayerAccountId <= 0)
            {
                throw new ArgumentException("Requester id must be positive.", nameof(requesterPlayerAccountId));
            }

            if (recipientPlayerAccountId <= 0)
            {
                throw new ArgumentException("Recipient id must be positive.", nameof(recipientPlayerAccountId));
            }

            if (requesterPlayerAccountId == recipientPlayerAccountId)
            {
                throw new ArgumentException("A player cannot send a friend request to themselves.", nameof(recipientPlayerAccountId));
            }
        }
    }
}
