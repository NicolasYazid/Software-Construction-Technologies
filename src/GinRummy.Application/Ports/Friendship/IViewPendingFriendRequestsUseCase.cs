using System.Collections.Generic;

using GinRummy.Domain.Entities.Friendship;

namespace GinRummy.Application.Ports.Friendship
{
    public interface IViewPendingFriendRequestsUseCase
    {
        IList<FriendRequest> GetPendingFriendRequests(int addresseeId);
    }
}
