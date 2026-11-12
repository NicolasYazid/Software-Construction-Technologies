using System;

using GinRummy.Application.Ports.Friendship;
using GinRummy.Contracts.Services.Friendship;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Services.Mappers.Friendship;
using ContractFriendRequest = GinRummy.Contracts.DataContracts.Friendship.FriendRequest;
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

            WcfCallbackDispatcher<IFriendshipCallback> addresseeDispatcher =
                _onlinePlayers.FindDispatcherOfPlayer(newRequest.AddresseePlayerAccountId);
            if (addresseeDispatcher == null)
            {
                return;
            }

            ContractFriendRequest receivedRequest = FriendshipMapper.MapToFriendRequest(newRequest, requesterProfile);
            addresseeDispatcher.EnqueueNotice(callback => callback.OnFriendRequestReceived(receivedRequest));
        }
    }
}
