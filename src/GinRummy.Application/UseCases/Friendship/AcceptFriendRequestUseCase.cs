using System;

using GinRummy.Application.Ports.Friendship;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Friendship;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Ports.Friendship;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.Friendship
{
    public class AcceptFriendRequestUseCase : IAcceptFriendRequestUseCase
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly IFriendshipAnnouncer _friendshipAnnouncer;

        public AcceptFriendRequestUseCase(Func<IUnitOfWork> unitOfWorkFactory, IFriendshipAnnouncer friendshipAnnouncer)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _friendshipAnnouncer = friendshipAnnouncer ?? throw new ArgumentNullException(nameof(friendshipAnnouncer));
        }

        public FriendRequestResponseResult AcceptFriendRequest(int addresseeId, int friendRequestId)
        {
            FriendRequestResponseResult result = FriendRequestResponseResult.RequestNotFound;
            bool isFriendShipCreated = false;
            PlayerProfile requesterProfile = null;
            PlayerProfile addresseeProfile;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                addresseeProfile = FindAddresseeProfile(unitOfWork, addresseeId);
                FriendRequest request = unitOfWork.Friends.FindRequestById(friendRequestId);
                if ((request != null) && (request.AddresseePlayerAccountId == addresseeId))
                {
                    result = FriendRequestRules.CheckAcceptance(unitOfWork.Friends, request.RequesterPlayerAccountId, addresseeId);
                    isFriendShipCreated = ApplyAcceptance(unitOfWork.Friends, request, result);
                    requesterProfile = unitOfWork.PlayerAccounts.FindProfileByPlayerAccountId(request.RequesterPlayerAccountId);
                    unitOfWork.Commit();
                }
            }

            if (isFriendShipCreated && (requesterProfile != null))
            {
                _friendshipAnnouncer.AnnounceFriendshipCreated(requesterProfile, addresseeProfile);
            }

            return result;
        }

        private static bool ApplyAcceptance(IFriendRepository friendRepository, FriendRequest request, FriendRequestResponseResult result)
        {
            bool isFriendShipCreated = false;
            int requesterId = request.RequesterPlayerAccountId;
            FriendShip existingFriendShip = friendRepository.FindFriendShip(requesterId, request.AddresseePlayerAccountId);
            if (result != FriendRequestResponseResult.AddresseeFriendLimitReached)
            {
                friendRepository.RemoveRequest(request.FriendRequestId);
            }

            if ((result == FriendRequestResponseResult.Accepted) && (existingFriendShip == null))
            {
                friendRepository.AddFriendShip(new FriendShip(requesterId, request.AddresseePlayerAccountId));
                isFriendShipCreated = true;
            }

            return isFriendShipCreated;
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
