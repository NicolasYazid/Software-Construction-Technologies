using System;

namespace GinRummy.Client.Models
{
    public sealed class FriendRequest
    {
        public FriendRequest(string senderName, DateTime createdAt)
        {
            SenderName = senderName ?? throw new ArgumentNullException(nameof(senderName));
            CreatedAt = createdAt;
        }

        public string SenderName { get; private set; }
        public DateTime CreatedAt { get; private set; }
    }
}
