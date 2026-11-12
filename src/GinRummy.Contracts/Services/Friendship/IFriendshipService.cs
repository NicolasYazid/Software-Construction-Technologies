using System.ServiceModel;

using GinRummy.Contracts.DataContracts.Friendship;
using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.Friendship
{
    [ServiceContract(CallbackContract = typeof(IFriendshipCallback), SessionMode = SessionMode.Required)]
    public interface IFriendshipService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SubscribeToFriendship(string accessToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendRequestOutcome SendFriendRequest(string accessToken, int addresseePublicTag);

        [OperationContract(IsOneWay = true)]
        void UnsubscribeFromFriendship();
    }
}
