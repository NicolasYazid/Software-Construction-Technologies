using System;

using GinRummy.Application.Ports.Friendship;
using GinRummy.Domain;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.Friendship
{
    public class RemoveFriendUseCase : IRemoveFriendUseCase
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly IFriendshipAnnouncer _friendshipAnnouncer;
        private readonly FriendRequestTimeLimits _friendRequestTimeLimits;

        public RemoveFriendUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            IFriendshipAnnouncer friendshipAnnouncer,
            FriendRequestTimeLimits friendRequestTimeLimits)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _friendshipAnnouncer = friendshipAnnouncer ?? throw new ArgumentNullException(nameof(friendshipAnnouncer));
            _friendRequestTimeLimits = friendRequestTimeLimits ?? throw new ArgumentNullException(nameof(friendRequestTimeLimits));
        }

        public void RemoveFriend(int playerId, int friendPublicTag)
        {
            bool isFriendShipRemoved = false;
            PlayerProfile playerProfile;
            PlayerProfile friendProfile;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                playerProfile = FindPlayerProfile(unitOfWork, playerId);
                friendProfile = unitOfWork.PlayerAccounts.FindByPublicTag(friendPublicTag);
                if (friendProfile != null)
                {
                    isFriendShipRemoved = RemoveExistingFriendShip(unitOfWork, playerId, friendProfile.PlayerAccountId);
                }
            }

            if (isFriendShipRemoved)
            {
                _friendRequestTimeLimits.RegisterRejectionOrRemoval(friendProfile.PlayerAccountId, playerId);
                _friendshipAnnouncer.AnnounceFriendRemoved(playerProfile, friendProfile);
            }
        }

        private static bool RemoveExistingFriendShip(IUnitOfWork unitOfWork, int playerId, int friendId)
        {
            bool isFriendShipRemoved = false;
            if (unitOfWork.Friends.FindFriendShip(playerId, friendId) != null)
            {
                unitOfWork.Friends.RemoveFriendShip(playerId, friendId);
                unitOfWork.Commit();
                isFriendShipRemoved = true;
            }

            return isFriendShipRemoved;
        }

        private static PlayerProfile FindPlayerProfile(IUnitOfWork unitOfWork, int playerId)
        {
            PlayerProfile playerProfile = unitOfWork.PlayerAccounts.FindProfileByPlayerAccountId(playerId);
            if (playerProfile == null)
            {
                throw new InvalidAccessTokenException("The access token does not belong to a registered player.");
            }

            return playerProfile;
        }
    }
}
