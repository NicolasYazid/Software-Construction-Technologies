namespace GinRummy.Domain.Entities.Friendship
{
    public enum FriendRequestResult
    {
        Sent,
        PlayerNotFound,
        SelfRequest,
        AlreadyFriends,
        RequestAlreadySent,
        IncomingRequestExists,
        AddresseeHasTooManyRequests,
        SenderFriendLimitReached,
        AddresseeFriendLimitReached,
        TooManyRequestsPerMinute,
        WaitingTimeNotOver
    }
}
