using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GinRummy.Client.FriendRequestResponseServiceReference;
using GinRummy.Client.FriendshipServiceReference;
using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;
using SavedFriendRequest = GinRummy.Client.PendingFriendRequestServiceReference.FriendRequest;

namespace GinRummy.Client.ViewModels
{
    public class NotificationsViewModel : ViewModelBase
    {
        private const string RequestAcceptedKey = "Notifications_RequestAccepted";
        private const string OwnFriendLimitKey = "Notifications_OwnFriendLimit";
        private const string RequesterFriendLimitKey = "Notifications_RequesterFriendLimit";

        private static readonly Dictionary<FriendRequestResponseOutcome, string> _noticeKeysByOutcome =
            new Dictionary<FriendRequestResponseOutcome, string>
            {
                { FriendRequestResponseOutcome.Accepted, RequestAcceptedKey },
                { FriendRequestResponseOutcome.AddresseeFriendLimitReached, OwnFriendLimitKey },
                { FriendRequestResponseOutcome.RequesterFriendLimitReached, RequesterFriendLimitKey }
            };

        private readonly FriendshipConnection _friendshipConnection;
        private readonly PendingFriendRequestsConnection _pendingRequestsConnection;
        private readonly FriendRequestResponseConnection _responseConnection;
        private bool _hasNoPendingRequests;
        private string _noticeKey;

        public NotificationsViewModel(
            FriendshipConnection friendshipConnection,
            PendingFriendRequestsConnection pendingRequestsConnection,
            FriendRequestResponseConnection responseConnection)
        {
            _friendshipConnection = friendshipConnection ?? throw new ArgumentNullException(nameof(friendshipConnection));
            _pendingRequestsConnection = pendingRequestsConnection
                ?? throw new ArgumentNullException(nameof(pendingRequestsConnection));
            _responseConnection = responseConnection ?? throw new ArgumentNullException(nameof(responseConnection));
            _pendingRequestsConnection.RequestsUnavailable += OnRequestsUnavailable;
            _responseConnection.ResponseUnavailable += OnRequestsUnavailable;
            FriendRequests = new ObservableCollection<PendingFriendRequestViewModel>();
            AcceptRequestCommand = new RelayCommand(AcceptRequest);
            DeclineRequestCommand = new RelayCommand(DeclineRequest);
            DismissNoticeCommand = new RelayCommand(() => NoticeKey = null);
            HasNoPendingRequests = FriendRequests.Count == 0;
        }

        public event EventHandler<ErrorDialogKind> RequestsUnavailable;

        public ObservableCollection<PendingFriendRequestViewModel> FriendRequests { get; private set; }
        public ICommand AcceptRequestCommand { get; private set; }
        public ICommand DeclineRequestCommand { get; private set; }
        public ICommand DismissNoticeCommand { get; private set; }

        public bool HasNoPendingRequests
        {
            get { return _hasNoPendingRequests; }
            private set
            {
                _hasNoPendingRequests = value;
                RaisePropertyChanged();
            }
        }

        public string NoticeKey
        {
            get { return _noticeKey; }
            private set
            {
                _noticeKey = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasNotice));
            }
        }

        public bool HasNotice
        {
            get { return _noticeKey != null; }
        }

        public void StartReceivingRequests()
        {
            _friendshipConnection.FriendshipEventReceived += OnFriendshipEventReceived;
        }

        public void StopReceivingRequests()
        {
            _friendshipConnection.FriendshipEventReceived -= OnFriendshipEventReceived;
        }

        public void LoadPendingRequests()
        {
            IList<SavedFriendRequest> savedRequests = _pendingRequestsConnection.GetPendingFriendRequests();
            FriendRequests.Clear();
            foreach (SavedFriendRequest savedRequest in savedRequests)
            {
                FriendRequests.Add(new PendingFriendRequestViewModel(savedRequest.FriendRequestId, savedRequest.RequesterUsername));
            }

            HasNoPendingRequests = FriendRequests.Count == 0;
        }

        private void AcceptRequest(object parameter)
        {
            PendingFriendRequestViewModel request = (PendingFriendRequestViewModel)parameter;
            FriendRequestResponseOutcome? outcome = _responseConnection.AcceptFriendRequest(request.FriendRequestId);
            if (outcome.HasValue)
            {
                ShowResponse(request, outcome.Value);
            }
        }

        private void DeclineRequest(object parameter)
        {
            PendingFriendRequestViewModel request = (PendingFriendRequestViewModel)parameter;
            FriendRequestResponseOutcome? outcome = _responseConnection.DeclineFriendRequest(request.FriendRequestId);
            if (outcome.HasValue)
            {
                ShowResponse(request, outcome.Value);
            }
        }

        private void ShowResponse(PendingFriendRequestViewModel request, FriendRequestResponseOutcome outcome)
        {
            if (outcome != FriendRequestResponseOutcome.AddresseeFriendLimitReached)
            {
                RemoveRequest(request);
            }

            string noticeKey;
            _noticeKeysByOutcome.TryGetValue(outcome, out noticeKey);
            NoticeKey = noticeKey;
        }

        private void RemoveRequest(PendingFriendRequestViewModel request)
        {
            FriendRequests.Remove(request);
            HasNoPendingRequests = FriendRequests.Count == 0;
        }

        private void OnFriendshipEventReceived(object sender, FriendshipEvent friendshipEvent)
        {
            if (friendshipEvent.EventType == FriendshipEventType.FriendRequestReceived)
            {
                AddRequestIfMissing(friendshipEvent.FriendRequestId, friendshipEvent.OtherPlayerUsername);
            }
        }

        private void AddRequestIfMissing(int friendRequestId, string requesterUsername)
        {
            bool isAlreadyListed = false;
            foreach (PendingFriendRequestViewModel listedRequest in FriendRequests)
            {
                if (listedRequest.FriendRequestId == friendRequestId)
                {
                    isAlreadyListed = true;
                }
            }

            if (!isAlreadyListed)
            {
                FriendRequests.Add(new PendingFriendRequestViewModel(friendRequestId, requesterUsername));
                HasNoPendingRequests = false;
            }
        }

        private void OnRequestsUnavailable(object sender, ErrorDialogKind errorDialogKind)
        {
            RequestsUnavailable?.Invoke(this, errorDialogKind);
        }
    }
}
