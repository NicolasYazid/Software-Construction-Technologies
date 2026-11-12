using GinRummy.Domain.Entities.Friendship;

namespace GinRummy.Application.Ports.Friendship
{
    public interface ISendFriendRequestUseCase
    {
        FriendRequestResult SendFriendRequest(int senderId, int addresseePublicTag);
    }
}
