using System;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class PendingFriendRequestViewModel : ViewModelBase
    {
        public PendingFriendRequestViewModel(int friendRequestId, string requesterUsername)
        {
            FriendRequestId = friendRequestId;
            RequesterUsername = requesterUsername ?? throw new ArgumentNullException(nameof(requesterUsername));
        }

        public int FriendRequestId { get; private set; }
        public string RequesterUsername { get; private set; }
    }
}
