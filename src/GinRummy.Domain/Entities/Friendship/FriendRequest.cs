using System;

using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Domain.Entities.Friendship
{
    public class FriendRequest
    {
        public FriendRequest(int requesterPlayerAccountId, int addresseePlayerAccountId, DateTime createdAt)
        {
            ValidatePlayers(requesterPlayerAccountId, addresseePlayerAccountId);

            RequesterPlayerAccountId = requesterPlayerAccountId;
            AddresseePlayerAccountId = addresseePlayerAccountId;
            CreatedAt = createdAt;
        }

        protected FriendRequest()
        {
        }

        public int FriendRequestId { get; private set; }
        public int RequesterPlayerAccountId { get; private set; }
        public int AddresseePlayerAccountId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public virtual PlayerAccount Requester { get; private set; }
        public virtual PlayerAccount Addressee { get; private set; }

        private static void ValidatePlayers(int requesterPlayerAccountId, int addresseePlayerAccountId)
        {
            if (requesterPlayerAccountId <= 0)
            {
                throw new ArgumentException("Requester id must be positive.", nameof(requesterPlayerAccountId));
            }

            if (addresseePlayerAccountId <= 0)
            {
                throw new ArgumentException("Addressee id must be positive.", nameof(addresseePlayerAccountId));
            }

            if (requesterPlayerAccountId == addresseePlayerAccountId)
            {
                throw new ArgumentException("A player cannot send a friend request to themselves.", nameof(addresseePlayerAccountId));
            }
        }
    }
}
