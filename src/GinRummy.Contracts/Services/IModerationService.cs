using System.ServiceModel;

using GinRummy.Contracts.Faults;

namespace GinRummy.Contracts.Services
{
    /// <summary>
    /// Operations to report a player for what they wrote in the chat (CU-18).
    /// </summary>
    [ServiceContract]
    public interface IModerationService
    {
        /// <summary>
        /// Reports the author of a chat message, starting from that message.
        /// </summary>
        /// <param name="accessToken">Access token of the player who reports.</param>
        /// <param name="chatMessageId">Identifier the chat gave to the reported message.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the access token is not valid, or when the chat no longer keeps the message.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ReportMessage(string accessToken, int chatMessageId);
    }
}
