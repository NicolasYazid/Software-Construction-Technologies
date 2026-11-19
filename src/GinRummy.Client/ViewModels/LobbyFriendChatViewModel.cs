using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GinRummy.Client.ChatMessageServiceReference;
using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class LobbyFriendChatViewModel : ViewModelBase
    {
        private const int MaxFriendMessages = 20;
        private const int OldestMessageIndex = 0;

        private readonly ChatMessageConnection _chatConnection;
        private readonly LogInOutcome _signedInPlayer;
        private readonly Dictionary<int, ObservableCollection<PlayerMessage>> _conversationsByFriendTag;
        private readonly HashSet<int> _friendTagsWithUnreadMessages;
        private LobbyPlayerViewModel _friend;
        private ObservableCollection<PlayerMessage> _messages;
        private string _messageDraft;

        public LobbyFriendChatViewModel(ChatMessageConnection chatConnection, LogInOutcome signedInPlayer)
        {
            _chatConnection = chatConnection ?? throw new ArgumentNullException(nameof(chatConnection));
            _signedInPlayer = signedInPlayer ?? throw new ArgumentNullException(nameof(signedInPlayer));
            _conversationsByFriendTag = new Dictionary<int, ObservableCollection<PlayerMessage>>();
            _friendTagsWithUnreadMessages = new HashSet<int>();
            _messages = new ObservableCollection<PlayerMessage>();
            _messageDraft = string.Empty;
            SendMessageCommand = new RelayCommand(SendMessage);
            CloseCommand = new RelayCommand(Close);
        }

        public event EventHandler UnreadMessagesChanged;
        public event EventHandler OpenConversationChanged;

        public ICommand SendMessageCommand { get; private set; }

        public ICommand CloseCommand { get; private set; }

        public ObservableCollection<PlayerMessage> Messages
        {
            get { return _messages; }
            private set
            {
                _messages = value;
                OnPropertyChanged();
            }
        }

        public LobbyPlayerViewModel Friend
        {
            get { return _friend; }
            private set
            {
                _friend = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsOpen));
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
                OnPropertyChanged();
            }
        }

        public void Open(LobbyPlayerViewModel friend)
        {
            Friend = friend ?? throw new ArgumentNullException(nameof(friend));
            int friendTag = friend.PublicTag.Value;
            Messages = GetConversation(friendTag);
            OpenConversationChanged?.Invoke(this, EventArgs.Empty);
            if (_friendTagsWithUnreadMessages.Remove(friendTag))
            {
                UnreadMessagesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ReceiveMessage(PlayerMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            int friendTag = FindFriendTag(message);
            AddMessageToConversation(GetConversation(friendTag), message);
            bool isConversationOpen = (_friend != null) && (_friend.PublicTag == friendTag) ? true : false;
            if (isConversationOpen)
            {
                OpenConversationChanged?.Invoke(this, EventArgs.Empty);
            }

            bool isFromFriend = message.PublicTag != _signedInPlayer.PublicTag;
            bool hasNewUnreadMessage = !isConversationOpen && isFromFriend ? true : false;
            if (hasNewUnreadMessage)
            {
                _friendTagsWithUnreadMessages.Add(friendTag);
                UnreadMessagesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void CloseConversationWith(int friendPublicTag)
        {
            if ((_friend != null) && (_friend.PublicTag == friendPublicTag))
            {
                Close();
            }
        }

        public bool HasUnreadMessages(int? friendPublicTag)
        {
            return friendPublicTag.HasValue && _friendTagsWithUnreadMessages.Contains(friendPublicTag.Value) ? true : false;
        }

        private void SendMessage()
        {
            string content = _messageDraft.Trim();
            bool canSendMessage = (_friend != null) && (content.Length > 0) ? true : false;
            if (canSendMessage)
            {
                MessageToSend message = new MessageToSend();
                message.Destination = MessageDestination.Friend;
                message.Content = content;
                message.FriendPublicTag = _friend.PublicTag.Value;
                _chatConnection.SendMessage(_signedInPlayer.AccessToken, message);
                MessageDraft = string.Empty;
            }
        }

        private void Close()
        {
            Friend = null;
        }

        private int FindFriendTag(PlayerMessage message)
        {
            return (message.PublicTag == _signedInPlayer.PublicTag) ? message.AddresseePublicTag : message.PublicTag;
        }

        private ObservableCollection<PlayerMessage> GetConversation(int friendTag)
        {
            ObservableCollection<PlayerMessage> conversation;
            if (!_conversationsByFriendTag.TryGetValue(friendTag, out conversation))
            {
                conversation = new ObservableCollection<PlayerMessage>();
                _conversationsByFriendTag.Add(friendTag, conversation);
            }

            return conversation;
        }

        private static void AddMessageToConversation(ObservableCollection<PlayerMessage> conversation, PlayerMessage message)
        {
            conversation.Add(message);
            while (conversation.Count > MaxFriendMessages)
            {
                conversation.RemoveAt(OldestMessageIndex);
            }
        }
    }
}