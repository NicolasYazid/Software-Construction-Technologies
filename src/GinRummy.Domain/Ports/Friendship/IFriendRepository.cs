using System.Collections.Generic;

using GinRummy.Domain.Entities.Friendship;

namespace GinRummy.Domain.Ports.Friendship
{
    public interface IFriendRepository
    {
        FriendRequest FindRequest(int requesterPlayerAccountId, int addresseePlayerAccountId);

        FriendRequest FindRequestById(int friendRequestId);

        IList<FriendRequest> GetRequestsReceivedBy(int addresseePlayerAccountId);

        int CountRequestsReceivedBy(int addresseePlayerAccountId);

        void AddRequest(FriendRequest newRequest);

        void RemoveRequest(int friendRequestId);

        FriendShip FindFriendShip(int firstPlayerAccountId, int secondPlayerAccountId);

        IList<FriendShip> GetFriendShipsOf(int playerAccountId);

        int CountFriendShipsOf(int playerAccountId);

        void AddFriendShip(FriendShip newFriendShip);

        void RemoveFriendShip(int firstPlayerAccountId, int secondPlayerAccountId);
    }
}
