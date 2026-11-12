using System;

using GinRummy.Application.Ports.Friendship;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Friendship;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.Friendship
{
    public class DeclineFriendRequestUseCase : IDeclineFriendRequestUseCase
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly IFriendshipAnnouncer _friendshipAnnouncer;
        private readonly FriendRequestTimeLimits _friendRequestTimeLimits;

        public DeclineFriendRequestUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            IFriendshipAnnouncer friendshipAnnouncer,
            FriendRequestTimeLimits friendRequestTimeLimits)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _friendshipAnnouncer = friendshipAnnouncer ?? throw new ArgumentNullException(nameof(friendshipAnnouncer));
            _friendRequestTimeLimits = friendRequestTimeLimits ?? throw new ArgumentNullException(nameof(friendRequestTimeLimits));
        }

        public FriendRequestResponseResult DeclineFriendRequest(int addresseeId, int friendRequestId)
        {
            FriendRequestResponseResult result = FriendRequestResponseResult.RequestNotFound;
            int requesterId = 0;
            PlayerProfile addresseeProfile;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                addresseeProfile = FindAddresseeProfile(unitOfWork, addresseeId);
                FriendRequest request = unitOfWork.Friends.FindRequestById(friendRequestId);
                if ((request != null) && (request.AddresseePlayerAccountId == addresseeId))
                {
                    requesterId = request.RequesterPlayerAccountId;
                    unitOfWork.Friends.RemoveRequest(friendRequestId);
                    unitOfWork.Commit();
                    result = FriendRequestResponseResult.Declined;
                }
            }

            if (result == FriendRequestResponseResult.Declined)
            {
                _friendRequestTimeLimits.RegisterRejectionOrRemoval(requesterId, addresseeId);
                _friendshipAnnouncer.AnnounceFriendRequestDeclined(requesterId, addresseeProfile);
            }

            return result;
        }

        private static PlayerProfile FindAddresseeProfile(IUnitOfWork unitOfWork, int addresseeId)
        {
            PlayerProfile addresseeProfile = unitOfWork.PlayerAccounts.FindProfileByPlayerAccountId(addresseeId);
            if (addresseeProfile == null)
            {
                throw new InvalidAccessTokenException("The access token does not belong to a registered player.");
            }

            return addresseeProfile;
        }
    }
}
