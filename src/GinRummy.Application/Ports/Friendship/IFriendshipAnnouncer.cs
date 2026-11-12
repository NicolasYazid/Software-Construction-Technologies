using GinRummy.Domain.Entities.Friendship;
using GinRummy.Domain.Entities.PlayerCustomization;

namespace GinRummy.Application.Ports.Friendship
{
    public interface IFriendshipAnnouncer
    {
        void AnnounceFriendRequestReceived(FriendRequest newRequest, PlayerProfile requesterProfile);

        void AnnounceFriendshipCreated(PlayerProfile requesterProfile, PlayerProfile addresseeProfile);

        void AnnounceFriendRequestDeclined(int requesterId, PlayerProfile declinerProfile);
    }
}
