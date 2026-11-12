using System;

using GinRummy.Application.Ports.Friendship;
using GinRummy.Contracts.DataContracts.Friendship;
using GinRummy.Contracts.Services.Friendship;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Services.Mappers.Friendship;
using DomainFriendRequest = GinRummy.Domain.Entities.Friendship.FriendRequest;

namespace GinRummy.Services.Notifiers
{
    public class FriendshipAnnouncer : IFriendshipAnnouncer
    {
        private readonly OnlinePlayers<IFriendshipCallback> _onlinePlayers;

        public FriendshipAnnouncer(OnlinePlayers<IFriendshipCallback> onlinePlayers)
        {
            _onlinePlayers = onlinePlayers ?? throw new ArgumentNullException(nameof(onlinePlayers));
        }

        public void AnnounceFriendRequestReceived(DomainFriendRequest newRequest, PlayerProfile requesterProfile)
        {
            if (newRequest == null)
            {
                throw new ArgumentNullException(nameof(newRequest));
            }

            if (requesterProfile == null)
            {
                throw new ArgumentNullException(nameof(requesterProfile));
            }

            FriendshipEvent eventForAddressee = FriendshipMapper.MapToReceivedFriendshipEvent(newRequest, requesterProfile);
            SendFriendshipEvent(newRequest.AddresseePlayerAccountId, eventForAddressee);
        }

        public void AnnounceFriendshipCreated(PlayerProfile requesterProfile, PlayerProfile addresseeProfile)
        {
            if (requesterProfile == null)
            {
                throw new ArgumentNullException(nameof(requesterProfile));
            }

            if (addresseeProfile == null)
            {
                throw new ArgumentNullException(nameof(addresseeProfile));
            }

            FriendshipEventType eventType = FriendshipEventType.FriendRequestAccepted;
            FriendshipEvent eventForRequester = FriendshipMapper.MapToFriendshipEvent(eventType, addresseeProfile);
            FriendshipEvent eventForAddressee = FriendshipMapper.MapToFriendshipEvent(eventType, requesterProfile);
            SendFriendshipEvent(requesterProfile.PlayerAccountId, eventForRequester);
            SendFriendshipEvent(addresseeProfile.PlayerAccountId, eventForAddressee);
        }

        public void AnnounceFriendRequestDeclined(int requesterId, PlayerProfile declinerProfile)
        {
            if (declinerProfile == null)
            {
                throw new ArgumentNullException(nameof(declinerProfile));
            }

            FriendshipEventType eventType = FriendshipEventType.FriendRequestDeclined;
            FriendshipEvent eventForRequester = FriendshipMapper.MapToFriendshipEvent(eventType, declinerProfile);
            SendFriendshipEvent(requesterId, eventForRequester);
        }

        private void SendFriendshipEvent(int playerId, FriendshipEvent friendshipEvent)
        {
            WcfCallbackDispatcher<IFriendshipCallback> playerDispatcher = _onlinePlayers.FindDispatcherOfPlayer(playerId);
            if (playerDispatcher != null)
            {
                playerDispatcher.EnqueueNotice(callback => callback.OnFriendshipEvent(friendshipEvent));
            }
        }
    }
}
