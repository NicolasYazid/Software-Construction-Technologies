namespace GinRummy.Domain.Entities.Friendship
{
    public enum FriendRequestResponseResult
    {
        Accepted,
        Declined,
        RequestNotFound,
        RequesterFriendLimitReached,
        AddresseeFriendLimitReached
    }
}
