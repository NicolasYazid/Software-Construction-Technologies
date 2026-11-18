using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GinRummy.Client.ChatMessageServiceReference;
using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class LobbyFriendChatViewModel : ViewModelBase
    {
        private LobbyPlayerViewModel _friend;
        private string _messageDraft;

        public LobbyFriendChatViewModel()
        {
            _messageDraft = string.Empty;
            Messages = new ObservableCollection<PlayerMessage>();
            CloseCommand = new RelayCommand(() => Friend = null);
        }

        public ObservableCollection<PlayerMessage> Messages { get; private set; }

        public ICommand SendMessageCommand { get; private set; }

        public ICommand CloseCommand { get; private set; }

        public LobbyPlayerViewModel Friend
        {
            get { return _friend; }
            private set
            {
                _friend = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsOpen));
            }
        }

        public bool IsOpen
        {
            get { return _friend != null; }
        }

        public string MessageDraft
        {
            get { return _messageDraft; }
            set
            {
                _messageDraft = value ?? string.Empty;
                RaisePropertyChanged();
            }
        }

        public void Open(LobbyPlayerViewModel friend)
        {
            Friend = friend ?? throw new ArgumentNullException(nameof(friend));
        }
    }
}