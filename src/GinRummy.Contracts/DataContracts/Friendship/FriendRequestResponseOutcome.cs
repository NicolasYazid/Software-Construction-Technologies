namespace GinRummy.Contracts.DataContracts.Friendship
{
    /// <summary>
    /// Answer to accepting or declining a friend request, so the client chooses what to show in the notifications panel.
    /// </summary>
    public enum FriendRequestResponseOutcome
    {
        /// <summary>
        /// The request was removed and the two players are friends now.
        /// </summary>
        Accepted,

        /// <summary>
        /// The request was removed without creating a friendship.
        /// </summary>
        Declined,

        /// <summary>
        /// The request does not exist or was not sent to the player who answers it, so nothing changed.
        /// </summary>
        RequestNotFound,

        /// <summary>
        /// The player who sent the request already has the maximum number of friends, so the request was removed.
        /// </summary>
        RequesterFriendLimitReached,

        /// <summary>
        /// The player who answers already has the maximum number of friends, so the request is still pending.
        /// </summary>
        AddresseeFriendLimitReached
    }
}
