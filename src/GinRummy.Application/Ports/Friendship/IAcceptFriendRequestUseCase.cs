using GinRummy.Domain.Entities.Friendship;

namespace GinRummy.Application.Ports.Friendship
{
    public interface IAcceptFriendRequestUseCase
    {
        FriendRequestResponseResult AcceptFriendRequest(int addresseeId, int friendRequestId);
    }
}
