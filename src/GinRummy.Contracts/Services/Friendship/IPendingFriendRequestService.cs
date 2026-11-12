using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Contracts.DataContracts.Friendship;
using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.Friendship
{
    [ServiceContract]
    public interface IPendingFriendRequestService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<FriendRequest> GetPendingFriendRequests(string accessToken);
    }
}
