namespace GinRummy.Contracts.DataContracts.Friendship
{
    public enum FriendRequestResponseOutcome
    {
        Accepted,
        Declined,
        RequestNotFound,
        RequesterFriendLimitReached,
        AddresseeFriendLimitReached
    }
}
