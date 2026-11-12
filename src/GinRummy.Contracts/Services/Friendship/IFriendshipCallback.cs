using System.ServiceModel;

using GinRummy.Contracts.DataContracts.Friendship;

namespace GinRummy.Contracts.Services.Friendship
{
    public interface IFriendshipCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnFriendRequestReceived(FriendRequest request);
    }
}
