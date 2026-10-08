using System.ComponentModel;

namespace GinRummy.Client.Models
{
    // It notifies the changes of its relation because the shared context menu does not rebuild itself when it opens again on the same row.
    public sealed class LobbyPlayerDto : INotifyPropertyChanged
    {
        private bool _isFriend;
        private bool _hasPendingRequest;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Username { get; set; }
        public string RankName { get; set; }

        public bool IsFriend
        {
            get { return _isFriend; }
            set
            {
                _isFriend = value;
                NotifyChanged(nameof(IsFriend));
            }
        }

        // A request still waiting for an answer turns the option of the menu into the mark of a pending request (CU-12).
        public bool HasPendingRequest
        {
            get { return _hasPendingRequest; }
            set
            {
                _hasPendingRequest = value;
                NotifyChanged(nameof(HasPendingRequest));
            }
        }

        private void NotifyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
