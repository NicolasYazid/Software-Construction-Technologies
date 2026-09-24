using System;

namespace GinRummy.Client.Models
{
    /// <summary>
    /// Friend request that waits for the answer of the player (CU-13).
    /// </summary>
    public sealed class FriendRequestDto
    {
        /// <summary>
        /// Gets or sets the name of the player who sent the request.
        /// </summary>
        public string SenderName { get; set; }
        /// <summary>
        /// Gets or sets the moment the request was sent, which orders the list.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
