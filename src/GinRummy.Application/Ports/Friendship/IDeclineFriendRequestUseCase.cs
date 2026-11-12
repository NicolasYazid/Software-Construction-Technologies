using GinRummy.Domain.Entities.Friendship;

namespace GinRummy.Application.Ports.Friendship
{
    public interface IDeclineFriendRequestUseCase
    {
        FriendRequestResponseResult DeclineFriendRequest(int addresseeId, int friendRequestId);
    }
}
