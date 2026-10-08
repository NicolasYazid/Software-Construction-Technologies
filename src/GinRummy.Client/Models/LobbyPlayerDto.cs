using System.ComponentModel;

namespace GinRummy.Client.Models
{
    // Player as the panel of players of the lobby lists them.
    // It announces the changes of its relation with whoever looks at the panel.
    // The context menu that reads them is shared by every row and does not rebuild itself when it opens again on the same one.
    public sealed class LobbyPlayerDto : INotifyPropertyChanged
    {
        private bool _isFriend;
        private bool _hasPendingRequest;

        public event PropertyChangedEventHandler PropertyChanged;

        // The name the player chose is never translated.
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
