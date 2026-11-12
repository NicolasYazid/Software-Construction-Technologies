namespace GinRummy.Contracts.DataContracts.Friendship
{
    /// <summary>
    /// What happened in the friendship between the player who receives the notice and another player.
    /// </summary>
    public enum FriendshipEventType
    {
        /// <summary>
        /// The other player sent a friend request to this player.
        /// </summary>
        FriendRequestReceived,

        /// <summary>
        /// A friend request between the two players was accepted, so they are friends now.
        /// </summary>
        FriendRequestAccepted,

        /// <summary>
        /// The other player declined the friend request this player sent.
        /// </summary>
        FriendRequestDeclined
    }
}
