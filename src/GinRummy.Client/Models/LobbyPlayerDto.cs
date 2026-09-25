using System.ComponentModel;

namespace GinRummy.Client.Models
{
    /// <summary>
    /// Player as the panel of players of the lobby lists them. It announces the changes of its
    /// relation with whoever looks at the panel, because the context menu that reads them is
    /// shared by every row and does not rebuild itself when it opens again on the same one.
    /// </summary>
    public sealed class LobbyPlayerDto : INotifyPropertyChanged
    {
        private bool _isFriend;
        private bool _hasPendingRequest;

        /// <summary>
        /// Occurs when the relation of the player with whoever looks at the panel changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the name the player chose, which is never translated.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the name of the rank of the player, as the catalogue of ranks delivers it.
        /// </summary>
        public string RankName { get; set; }

        /// <summary>
        /// Gets or sets whether the player is a friend of whoever looks at the panel, which
        /// decides the actions its context menu offers.
        /// </summary>
        public bool IsFriend
        {
            get { return _isFriend; }
            set
            {
                _isFriend = value;
                NotifyChanged(nameof(IsFriend));
            }
        }

        /// <summary>
        /// Gets or sets whether a friend request sent to the player is still waiting for an
        /// answer, which turns the option of the menu into the mark of a pending request
        /// (CU-12).
        /// </summary>
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
