using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class NotificationsViewModel : ViewModelBase
    {
        private bool _hasNoPendingRequests;

        public NotificationsViewModel(SampleDataService dataService)
        {
            if (dataService == null)
            {
                throw new ArgumentNullException(nameof(dataService));
            }

            FriendRequests = new ObservableCollection<FriendRequest>(dataService.GetFriendRequests());
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

        // The screen only takes the request off the list because the server creates the friendship (CU-14).
        private void AcceptRequest(object parameter)
        {
            RemoveRequest(parameter as FriendRequest);
        }

        private void DeclineRequest(object parameter)
        {
            RemoveRequest(parameter as FriendRequest);
        }

        private void RemoveRequest(FriendRequest request)
        {
            FriendRequests.Remove(request);
            HasNoPendingRequests = FriendRequests.Count == 0;
        }
    }
}
