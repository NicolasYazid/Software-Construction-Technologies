namespace GinRummy.Contracts.DataContracts.Friendship
{
    public enum FriendRequestOutcome
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
