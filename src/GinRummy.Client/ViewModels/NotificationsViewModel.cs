using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;
using LiveFriendRequest = GinRummy.Client.FriendshipServiceReference.FriendRequest;
using SavedFriendRequest = GinRummy.Client.PendingFriendRequestServiceReference.FriendRequest;

namespace GinRummy.Client.ViewModels
{
    public class NotificationsViewModel : ViewModelBase
    {
        private readonly FriendshipConnection _friendshipConnection;
        private readonly PendingFriendRequestsConnection _pendingRequestsConnection;
        private bool _hasNoPendingRequests;

        public NotificationsViewModel(FriendshipConnection friendshipConnection, PendingFriendRequestsConnection pendingRequestsConnection)
        {
            _friendshipConnection = friendshipConnection ?? throw new ArgumentNullException(nameof(friendshipConnection));
            _pendingRequestsConnection = pendingRequestsConnection
                ?? throw new ArgumentNullException(nameof(pendingRequestsConnection));
            _pendingRequestsConnection.RequestsUnavailable += OnRequestsUnavailable;
            FriendRequests = new ObservableCollection<PendingFriendRequestViewModel>();
            AcceptRequestCommand = new RelayCommand(AcceptRequest);
            DeclineRequestCommand = new RelayCommand(DeclineRequest);
            HasNoPendingRequests = FriendRequests.Count == 0;
        }

        public event EventHandler<ErrorDialogKind> RequestsUnavailable;

        public ObservableCollection<PendingFriendRequestViewModel> FriendRequests { get; private set; }
        public ICommand AcceptRequestCommand { get; private set; }
        public ICommand DeclineRequestCommand { get; private set; }

        public bool HasNoPendingRequests
        {
            get { return _hasNoPendingRequests; }
            private set
            {
                _hasNoPendingRequests = value;
                RaisePropertyChanged();
            }
        }

        public void StartReceivingRequests()
        {
            _friendshipConnection.FriendRequestReceived += OnFriendRequestReceived;
        }

        public void StopReceivingRequests()
        {
            _friendshipConnection.FriendRequestReceived -= OnFriendRequestReceived;
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

        // The screen only takes the request off the list because the server creates the friendship (CU-14).
        private void AcceptRequest(object parameter)
        {
            RemoveRequest((PendingFriendRequestViewModel)parameter);
        }

        private void DeclineRequest(object parameter)
        {
            RemoveRequest((PendingFriendRequestViewModel)parameter);
        }

        private void RemoveRequest(PendingFriendRequestViewModel request)
        {
            FriendRequests.Remove(request);
            HasNoPendingRequests = FriendRequests.Count == 0;
        }

        private void OnFriendRequestReceived(object sender, LiveFriendRequest request)
        {
            AddRequestIfMissing(request.FriendRequestId, request.RequesterUsername);
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
