using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GinRummy.Client.ChatMessageServiceReference;
using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class MatchChatViewModel : ViewModelBase
    {
        private const int MaxMatchMessages = 20;
        private const int OldestMessageIndex = 0;

        private readonly ChatMessageConnection _chatConnection;
        private readonly LogInOutcome _signedInPlayer;
        private string _messageDraft;

        public MatchChatViewModel(ChatMessageConnection chatConnection, LogInOutcome signedInPlayer)
        {
            _chatConnection = chatConnection ?? throw new ArgumentNullException(nameof(chatConnection));
            _signedInPlayer = signedInPlayer ?? throw new ArgumentNullException(nameof(signedInPlayer));
            _messageDraft = string.Empty;
            Messages = new ObservableCollection<PlayerMessage>();
            SendMessageCommand = new RelayCommand(SendMessage);
        }

        public ObservableCollection<PlayerMessage> Messages { get; private set; }
        public ICommand SendMessageCommand { get; private set; }

        public string MessageDraft
        {
            get { return _messageDraft; }
            set
            {
                _messageDraft = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public void ReceiveMessage(PlayerMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Messages.Add(message);
            while (Messages.Count > MaxMatchMessages)
            {
                Messages.RemoveAt(OldestMessageIndex);
            }
        }

        public void ClearMessages()
        {
            Messages.Clear();
            MessageDraft = string.Empty;
        }

        private void SendMessage()
        {
            string content = _messageDraft.Trim();
            if (content.Length > 0)
            {
                MessageToSend message = new MessageToSend();
                message.Destination = MessageDestination.Match;
                message.Content = content;
                _chatConnection.SendMessage(_signedInPlayer.AccessToken, message);
                MessageDraft = string.Empty;
            }
        }
    }
}
