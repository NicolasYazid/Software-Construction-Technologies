using System.ServiceModel;

using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.PlayerModeration
{
    [ServiceContract]
    public interface IModerationService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ReportMessage(string accessToken, int chatMessageId);
    }
}
