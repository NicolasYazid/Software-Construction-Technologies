using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Contracts.DataContracts;

namespace GinRummy.Contracts.Callbacks
{
    /// <summary>
    /// Calls the server makes to the client to fill its chats. The server does not wait for the client to answer, so a
    /// slow client does not hold back the messages of the other players.
    /// </summary>
    public interface IChatMessageCallback
    {
        /// <summary>
        /// Delivers the last messages of the lobby, from the oldest to the newest. It arrives once, right after subscribing.
        /// </summary>
        /// <param name="messages">Messages of the lobby; empty when nobody has written since the server started.</param>
        [OperationContract(IsOneWay = true)]
        void OnLobbyChatHistoryReceived(List<PlayerMessage> messages);

        /// <summary>
        /// Delivers a new message of the lobby or of the match.
        /// </summary>
        /// <param name="message">Message to show in the chat its destination names.</param>
        [OperationContract(IsOneWay = true)]
        void OnChatMessageReceived(PlayerMessage message);
    }
}
