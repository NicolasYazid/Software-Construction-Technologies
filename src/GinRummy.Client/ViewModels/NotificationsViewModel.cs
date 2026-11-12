using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GinRummy.Client.FriendshipServiceReference;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class NotificationsViewModel : ViewModelBase
    {
        private readonly FriendshipConnection _friendshipConnection;
        private bool _hasNoPendingRequests;

        public NotificationsViewModel(FriendshipConnection friendshipConnection)
        {
            _friendshipConnection = friendshipConnection ?? throw new ArgumentNullException(nameof(friendshipConnection));
            FriendRequests = new ObservableCollection<FriendRequest>();
            AcceptRequestCommand = new RelayCommand(AcceptRequest);
            DeclineRequestCommand = new RelayCommand(DeclineRequest);
            HasNoPendingRequests = FriendRequests.Count == 0;
        }

        public ObservableCollection<FriendRequest> FriendRequests { get; private set; }
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

        // The screen only takes the request off the list because the server creates the friendship (CU-14).
        private void AcceptRequest(object parameter)
        {
            RemoveRequest((FriendRequest)parameter);
        }

        private void DeclineRequest(object parameter)
        {
            RemoveRequest((FriendRequest)parameter);
        }

        private void RemoveRequest(FriendRequest request)
        {
            FriendRequests.Remove(request);
            HasNoPendingRequests = FriendRequests.Count == 0;
        }

        private void OnFriendRequestReceived(object sender, FriendRequest request)
        {
            FriendRequests.Insert(0, request);
            HasNoPendingRequests = false;
        }
    }
}
