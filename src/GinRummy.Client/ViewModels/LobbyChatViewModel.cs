using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GinRummy.Client.ChatMessageServiceReference;
using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.PlayerPresenceServiceReference;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class LobbyChatViewModel : ViewModelBase
    {
        private readonly PlayerPresenceMonitor _presenceMonitor;
        private readonly ChatMessageConnection _chatConnection;
        private readonly LogInOutcome _signedInPlayer;
        private string _messageDraft;
        private bool _isSearchingMatch;
        private bool _isChatBlocked;
        private bool _isChatUnavailable;
        private bool _hasNoFriends;
        private string _banReasonName;

        public LobbyChatViewModel(
            PlayerPresenceMonitor presenceMonitor,
            ChatMessageConnection chatConnection,
            LogInOutcome signedInPlayer)
        {
            _presenceMonitor = presenceMonitor ?? throw new ArgumentNullException(nameof(presenceMonitor));
            _chatConnection = chatConnection ?? throw new ArgumentNullException(nameof(chatConnection));
            _signedInPlayer = signedInPlayer ?? throw new ArgumentNullException(nameof(signedInPlayer));
            _messageDraft = string.Empty;
            ChatEntries = new ObservableCollection<PlayerMessage>();
            LookingToPlay = new ObservableCollection<LobbyPlayerViewModel>();
            Online = new ObservableCollection<LobbyPlayerViewModel>();
            MatchesInProgress = new ObservableCollection<MatchPair>();
            Unavailable = new ObservableCollection<LobbyPlayerViewModel>();
            FriendsOnline = new ObservableCollection<LobbyPlayerViewModel>();
            FriendsInMatch = new ObservableCollection<MatchPair>();
            FriendsUnavailable = new ObservableCollection<LobbyPlayerViewModel>();
            HasNoPlayersLookingToPlay = (LookingToPlay.Count == 0);
            _hasNoFriends = HasNoFriendsListed();
            BanCountdown = new CountdownViewModel(TimeSpan.Zero);
            SendMessageCommand = new RelayCommand(SendMessage);
            FindMatchCommand = new RelayCommand(FindMatch);
            CancelSearchCommand = new RelayCommand(CancelSearch);
            FriendRequestCommand = new RelayCommand(SendFriendRequest);
        }

        public event EventHandler PresenceUnavailable;

        public ObservableCollection<PlayerMessage> ChatEntries { get; private set; }
        public bool HasNoPlayersLookingToPlay { get; private set; }
        public CountdownViewModel BanCountdown { get; private set; }
        public ICommand SendMessageCommand { get; private set; }
        public ICommand FindMatchCommand { get; private set; }
        public ICommand CancelSearchCommand { get; private set; }
        public ICommand FriendRequestCommand { get; private set; }
        public IList<LobbyPlayerViewModel> LookingToPlay { get; private set; }
        public IList<LobbyPlayerViewModel> Online { get; private set; }
        public IList<MatchPair> MatchesInProgress { get; private set; }
        public int PlayersInMatchCount { get; private set; }
        public IList<LobbyPlayerViewModel> Unavailable { get; private set; }
        public IList<LobbyPlayerViewModel> FriendsOnline { get; private set; }
        public IList<MatchPair> FriendsInMatch { get; private set; }
        public int FriendsInMatchCount { get; private set; }
        public IList<LobbyPlayerViewModel> FriendsUnavailable { get; private set; }

        public string MessageDraft
        {
            get { return _messageDraft; }
            set
            {
                _messageDraft = value ?? string.Empty;
                RaisePropertyChanged();
            }
        }

        public bool IsSearchingMatch
        {
            get { return _isSearchingMatch; }
            private set
            {
                _isSearchingMatch = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanFindMatch));
            }
        }

        public bool CanFindMatch
        {
            get { return !_isSearchingMatch; }
        }

        public bool IsChatBlocked
        {
            get { return _isChatBlocked; }
            private set
            {
                _isChatBlocked = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanWriteMessage));
            }
        }

        public bool CanWriteMessage
        {
            get { return !_isChatBlocked && !_isChatUnavailable; }
        }

        public string BanReasonName
        {
            get { return _banReasonName; }
            private set
            {
                _banReasonName = value;
                RaisePropertyChanged();
            }
        }

        public bool HasNoFriends
        {
            get { return _hasNoFriends; }
            private set
            {
                _hasNoFriends = value;
                RaisePropertyChanged();
            }
        }

        // Blocks the chat of a player whose sanction restricts it (CU-17 EX-08).
        public void BlockChat(string reasonName, TimeSpan remainingTime)
        {
            BanReasonName = reasonName;
            IsChatBlocked = true;
            BanCountdown.Restart(remainingTime);
        }

        public void StopCountdown()
        {
            BanCountdown.Stop();
        }

        // The server reports the current status of the players once the subscription is made, and every change after that.
        public void StartPresence()
        {
            _presenceMonitor.CurrentStatusReported += OnCurrentStatusReported;
            _presenceMonitor.PlayerPresenceChanged += OnPlayerPresenceChanged;
            if (!_presenceMonitor.Start(_signedInPlayer.AccessToken))
            {
                PresenceUnavailable?.Invoke(this, EventArgs.Empty);
            }
        }

        public void StopPresence()
        {
            _presenceMonitor.Stop();
            _presenceMonitor.CurrentStatusReported -= OnCurrentStatusReported;
            _presenceMonitor.PlayerPresenceChanged -= OnPlayerPresenceChanged;
        }

        public void StartChat()
        {
            _chatConnection.LobbyChatHistoryReceived += OnLobbyChatHistoryReceived;
            _chatConnection.ChatMessageReceived += OnChatMessageReceived;
            if (!_chatConnection.Start(_signedInPlayer.AccessToken))
            {
                _isChatUnavailable = true;
                RaisePropertyChanged(nameof(CanWriteMessage));
            }
        }

        public void StopChat()
        {
            _chatConnection.Stop();
            _chatConnection.LobbyChatHistoryReceived -= OnLobbyChatHistoryReceived;
            _chatConnection.ChatMessageReceived -= OnChatMessageReceived;
        }

        // Only the messages of other players can be reported, so the own messages open no menu.
        public bool CanReport(PlayerMessage message)
        {
            return (message != null) && (message.PublicTag != _signedInPlayer.PublicTag);
        }

        // The friend leaves the friends tab, as CU-16 Post-2 and Post-3 require.
        // Every other row that lists the player offers a friend request again instead of the removal.
        public void RemoveFriend(string username)
        {
            RemoveByName(FriendsOnline, username);
            RemoveByName(FriendsUnavailable, username);
            IEnumerable<LobbyPlayerViewModel> otherRows = LookingToPlay
                .Concat(Online)
                .Concat(Unavailable)
                .Where(candidate => candidate.Username == username);
            foreach (LobbyPlayerViewModel player in otherRows)
            {
                player.IsFriend = false;
            }

            HasNoFriends = HasNoFriendsListed();
        }

        // An empty message is not sent and shows no error (CU-17 FA-03).
        private void SendMessage()
        {
            string content = _messageDraft.Trim();
            if (CanWriteMessage && (content.Length > 0))
            {
                _chatConnection.SendLobbyMessage(_signedInPlayer.AccessToken, content);
                MessageDraft = string.Empty;
            }
        }

        // The search stays in the state of CU-22 FA-03, the one the prototype draws, until the server pairs the players.
        private void FindMatch()
        {
            IsSearchingMatch = true;
        }

        private void CancelSearch()
        {
            IsSearchingMatch = false;
        }

        // The option turns into the pending request mark so a second request cannot be sent while the first one waits (CU-12 step 8).
        private void SendFriendRequest(object parameter)
        {
            LobbyPlayerViewModel player = parameter as LobbyPlayerViewModel;
            if (player != null)
            {
                player.HasPendingRequest = true;
            }
        }

        private void OnCurrentStatusReported(object sender, PlayerPresenceReport report)
        {
            ReplacePlayers(Online, report.AllOnlinePlayers);
            ReplacePlayers(Unavailable, report.AllOfflinePlayers);
            ReplacePlayers(FriendsOnline, report.AllOnlineFriends);
            ReplacePlayers(FriendsUnavailable, report.AllOfflineFriends);
            HasNoFriends = HasNoFriendsListed();
        }

        // A player is listed in one status only, so its old row is removed before the new one is added.
        private void OnPlayerPresenceChanged(object sender, PlayerPresence presence)
        {
            RemoveByPublicTag(Online, presence.PublicTag);
            RemoveByPublicTag(Unavailable, presence.PublicTag);
            RemoveByPublicTag(FriendsOnline, presence.PublicTag);
            RemoveByPublicTag(FriendsUnavailable, presence.PublicTag);
            if (presence.Status == PresenceStatus.Online)
            {
                AddPlayer(Online, FriendsOnline, presence);
            }
            else
            {
                AddPlayer(Unavailable, FriendsUnavailable, presence);
            }

            HasNoFriends = HasNoFriendsListed();
        }

        private void OnLobbyChatHistoryReceived(object sender, PlayerMessage[] messages)
        {
            foreach (PlayerMessage message in messages)
            {
                ChatEntries.Add(message);
            }
        }

        private void OnChatMessageReceived(object sender, PlayerMessage message)
        {
            if (message.Destination == MessageDestination.Lobby)
            {
                ChatEntries.Add(message);
            }
        }

        private static void RemoveByName(IList<LobbyPlayerViewModel> players, string username)
        {
            LobbyPlayerViewModel player = players.FirstOrDefault(candidate => candidate.Username == username);
            if (player != null)
            {
                players.Remove(player);
            }
        }

        private bool HasNoFriendsListed()
        {
            return (FriendsOnline.Count == 0)
                && (FriendsInMatch.Count == 0)
                && (FriendsUnavailable.Count == 0);
        }

        private static void ReplacePlayers(IList<LobbyPlayerViewModel> players, IList<PlayerPresence> presences)
        {
            players.Clear();
            foreach (PlayerPresence presence in presences)
            {
                players.Add(CreatePlayer(presence));
            }
        }

        // A friend is listed in the group of everyone and in the one of friends, each with its own row.
        private static void AddPlayer(
            IList<LobbyPlayerViewModel> everyone,
            IList<LobbyPlayerViewModel> friends,
            PlayerPresence presence)
        {
            everyone.Add(CreatePlayer(presence));
            if (presence.IsFriend)
            {
                friends.Add(CreatePlayer(presence));
            }
        }

        private static void RemoveByPublicTag(IList<LobbyPlayerViewModel> players, int publicTag)
        {
            LobbyPlayerViewModel player = players.FirstOrDefault(candidate => candidate.PublicTag == publicTag);
            if (player != null)
            {
                players.Remove(player);
            }
        }

        // The presence service does not send the rank, so the row keeps the neutral badge until another service provides it.
        private static LobbyPlayerViewModel CreatePlayer(PlayerPresence presence)
        {
            LobbyPlayerViewModel player = new LobbyPlayerViewModel();
            player.Username = presence.Username;
            player.PublicTag = presence.PublicTag;
            player.IsFriend = presence.IsFriend;

            return player;
        }
    }
}
