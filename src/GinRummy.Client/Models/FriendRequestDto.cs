using System;

namespace GinRummy.Client.Models
{
    // Friend request that waits for the answer of the player (CU-13).
    public sealed class FriendRequestDto
    {
        public string SenderName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
