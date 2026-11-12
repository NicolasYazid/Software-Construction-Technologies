using System;
using System.Collections.Generic;

using GinRummy.Application.Ports.Friendship;
using GinRummy.Domain.Entities.Friendship;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.Friendship
{
    public class ViewPendingFriendRequestsUseCase : IViewPendingFriendRequestsUseCase
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;

        public ViewPendingFriendRequestsUseCase(Func<IUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public IList<FriendRequest> GetPendingFriendRequests(int addresseeId)
        {
            IList<FriendRequest> pendingRequests;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                pendingRequests = unitOfWork.Friends.GetRequestsReceivedBy(addresseeId);
            }

            return pendingRequests;
        }
    }
}
