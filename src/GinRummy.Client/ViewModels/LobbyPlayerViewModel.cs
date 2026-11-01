using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    // It notifies the changes of its relation because the shared context menu does not rebuild itself when it opens again on the same row.
    public sealed class LobbyPlayerViewModel : ViewModelBase
    {
        private bool _isFriend;
        private bool _hasPendingRequest;
        private bool _isOnline;

        public string Username { get; set; }
        public string RankName { get; set; }

        // The public tag identifies the friend across presence notifications (CU-11 real-time presence).
        public int PublicTag { get; set; }

        // Presence state of the friend: it decides the Online or Unavailable group of the friends tab.
        public bool IsOnline
        {
            get { return _isOnline; }
            set
            {
                _isOnline = value;
                RaisePropertyChanged();
            }
        }

        public bool IsFriend
        {
            get { return _isFriend; }
            set
            {
                _isFriend = value;
                RaisePropertyChanged();
            }
        }

        // A request still waiting for an answer turns the option of the menu into the mark of a pending request (CU-12).
        public bool HasPendingRequest
        {
            get { return _hasPendingRequest; }
            set
            {
                _hasPendingRequest = value;
                RaisePropertyChanged();
            }
        }
    }
}
