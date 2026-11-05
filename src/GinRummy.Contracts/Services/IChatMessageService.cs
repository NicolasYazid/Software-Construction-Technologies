using System.ServiceModel;

using GinRummy.Contracts.Callbacks;
using GinRummy.Contracts.Faults;

namespace GinRummy.Contracts.Services
{
    /// <summary>
    /// Chat of the lobby and of the matches (CU-17). The connection stays open so that the server can push the messages of
    /// the other players through IChatMessageCallback.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IChatMessageCallback), SessionMode = SessionMode.Required)]
    public interface IChatMessageService
    {
        /// <summary>
        /// Starts receiving the messages of the chat. The last messages of the lobby arrive right after through
        /// OnLobbyChatHistoryReceived.
        /// </summary>
        /// <param name="accessToken">Access token granted at sign-in.</param>
        /// <exception cref="FaultException{ServiceFault}">Thrown when the access token is altered or expired.</exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SubscribeToChat(string accessToken);

        /// <summary>
        /// Sends a message to every player subscribed to the chat. An empty text or one longer than 300 characters is
        /// discarded without an answer, because the client never sends them.
        /// </summary>
        /// <param name="accessToken">Access token granted at sign-in.</param>
        /// <param name="content">Text exactly as the player wrote it.</param>
        /// <exception cref="FaultException{ServiceFault}">Thrown when the access token is altered or expired.</exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SendLobbyMessage(string accessToken, string content);

        /// <summary>
        /// Sends a message to the rival of the match the player is playing. It is discarded without an answer when the text
        /// is empty or longer than 300 characters, or when the match already ended.
        /// </summary>
        /// <param name="accessToken">Access token granted at sign-in.</param>
        /// <param name="content">Text exactly as the player wrote it.</param>
        /// <exception cref="FaultException{ServiceFault}">Thrown when the access token is altered or expired.</exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SendMatchMessage(string accessToken, string content);

        /// <summary>
        /// Stops receiving the messages of the chat. Closing the connection has the same effect.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void UnsubscribeFromChat();
    }
}
