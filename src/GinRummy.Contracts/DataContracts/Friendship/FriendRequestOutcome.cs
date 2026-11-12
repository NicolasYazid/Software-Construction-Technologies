namespace GinRummy.Contracts.DataContracts.Friendship
{
    /// <summary>
    /// Answer to a friend request: either it was sent, or the rule that rejected it, so the client chooses its message.
    /// </summary>
    public enum FriendRequestOutcome
    {
        /// <summary>
        /// The request was stored and the other player receives it if connected.
        /// </summary>
        Sent,

        /// <summary>
        /// No registered player has the public tag.
        /// </summary>
        PlayerNotFound,

        /// <summary>
        /// The public tag belongs to the player who sends the request.
        /// </summary>
        SelfRequest,

        /// <summary>
        /// The two players are already friends.
        /// </summary>
        AlreadyFriends,

        /// <summary>
        /// The player already sent a request to the other player and it is still pending.
        /// </summary>
        RequestAlreadySent,

        /// <summary>
        /// The other player already sent a pending request to this player, which can be accepted instead (CU-12 FA-04).
        /// </summary>
        IncomingRequestExists,

        /// <summary>
        /// The other player already has the maximum number of pending requests.
        /// </summary>
        AddresseeHasTooManyRequests,

        /// <summary>
        /// The player who sends the request already has the maximum number of friends.
        /// </summary>
        SenderFriendLimitReached,

        /// <summary>
        /// The other player already has the maximum number of friends.
        /// </summary>
        AddresseeFriendLimitReached,

        /// <summary>
        /// The player already sent the maximum number of requests allowed in one minute.
        /// </summary>
        TooManyRequestsPerMinute,

        /// <summary>
        /// The other player rejected a request of this player, or removed this player from the friends, too recently.
        /// </summary>
        WaitingTimeNotOver
    }
}
