using System.ServiceModel;

using GinRummy.Contracts.DataContracts.Friendship;
using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.Friendship
{
    [ServiceContract]
    public interface IFriendRequestResponseService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendRequestResponseOutcome AcceptFriendRequest(string accessToken, int friendRequestId);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendRequestResponseOutcome DeclineFriendRequest(string accessToken, int friendRequestId);
    }
}
