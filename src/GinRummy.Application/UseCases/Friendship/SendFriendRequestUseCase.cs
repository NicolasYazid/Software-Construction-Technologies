using System;

using GinRummy.Application.Ports.Friendship;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Friendship;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Ports.Friendship;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.Friendship
{
    public class SendFriendRequestUseCase : ISendFriendRequestUseCase
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly IFriendshipAnnouncer _friendshipAnnouncer;
        private readonly FriendRequestTimeLimits _friendRequestTimeLimits;

        public SendFriendRequestUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            IFriendshipAnnouncer friendshipAnnouncer,
            FriendRequestTimeLimits friendRequestTimeLimits)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _friendshipAnnouncer = friendshipAnnouncer ?? throw new ArgumentNullException(nameof(friendshipAnnouncer));
            _friendRequestTimeLimits = friendRequestTimeLimits ?? throw new ArgumentNullException(nameof(friendRequestTimeLimits));
        }

        public FriendRequestResult SendFriendRequest(int senderId, int addresseePublicTag)
        {
            FriendRequestResult result = FriendRequestResult.TooManyRequestsPerMinute;
            if (_friendRequestTimeLimits.CanSendAnotherRequest(senderId))
            {
                result = StoreFriendRequest(senderId, addresseePublicTag);
            }

            return result;
        }

        private FriendRequestResult StoreFriendRequest(int senderId, int addresseePublicTag)
        {
            FriendRequestResult result = FriendRequestResult.PlayerNotFound;
            PlayerProfile senderProfile;
            FriendRequest newRequest = null;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                senderProfile = FindSenderProfile(unitOfWork, senderId);
                PlayerProfile addresseeProfile = unitOfWork.PlayerAccounts.FindByPublicTag(addresseePublicTag);
                if (addresseeProfile != null)
                {
                    result = CheckRequestToAddressee(unitOfWork.Friends, senderId, addresseeProfile.PlayerAccountId);
                }

                if (result == FriendRequestResult.Sent)
                {
                    DateTime sentAt = _friendRequestTimeLimits.RegisterSentRequest(senderId);
                    newRequest = new FriendRequest(senderId, addresseeProfile.PlayerAccountId, sentAt);
                    unitOfWork.Friends.AddRequest(newRequest);
                    unitOfWork.Commit();
                }
            }

            if (result == FriendRequestResult.Sent)
            {
                _friendshipAnnouncer.AnnounceFriendRequestReceived(newRequest, senderProfile);
            }

            return result;
        }

        private FriendRequestResult CheckRequestToAddressee(IFriendRepository friendRepository, int senderId, int addresseeId)
        {
            FriendRequestResult result = FriendRequestResult.WaitingTimeNotOver;
            if (_friendRequestTimeLimits.HasWaitingTimeEnded(senderId, addresseeId))
            {
                result = FriendRequestRules.CheckNewRequest(friendRepository, senderId, addresseeId);
            }

            return result;
        }

        private static PlayerProfile FindSenderProfile(IUnitOfWork unitOfWork, int senderId)
        {
            PlayerProfile senderProfile = unitOfWork.PlayerAccounts.FindProfileByPlayerAccountId(senderId);
            if (senderProfile == null)
            {
                throw new InvalidAccessTokenException("The access token does not belong to a registered player.");
            }

            return senderProfile;
        }
    }
}
