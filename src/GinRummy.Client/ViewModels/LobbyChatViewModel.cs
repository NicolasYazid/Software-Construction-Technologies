using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class LobbyChatViewModel : ViewModelBase
    {
        private readonly SampleDataService _dataService;
        private readonly LobbySnapshot _lobby;
        private string _messageDraft;
        private bool _isSearchingMatch;
        private bool _isChatBlocked;
        private bool _hasNoFriends;
        private string _banReasonName;

        public LobbyChatViewModel(SampleDataService dataService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _lobby = dataService.GetLobby();
            _messageDraft = string.Empty;
            ChatEntries = new ObservableCollection<ChatEntry>(_lobby.ChatEntries);
            HasNoPlayersLookingToPlay = _lobby.LookingToPlay.Count == 0;
            _hasNoFriends = HasNoFriendsIn(_lobby);
            BanCountdown = new CountdownViewModel(TimeSpan.Zero);
            SendMessageCommand = new RelayCommand(SendMessage);
            FindMatchCommand = new RelayCommand(FindMatch);
            CancelSearchCommand = new RelayCommand(CancelSearch);
            ChallengeCommand = new RelayCommand(Challenge);
            FriendRequestCommand = new RelayCommand(SendFriendRequest);
            AcceptChallengeCommand = new RelayCommand(AcceptChallenge);
            DeclineChallengeCommand = new RelayCommand(RemoveChatEntry);
            CancelChallengeCommand = new RelayCommand(RemoveChatEntry);
        }

        public event EventHandler MatchStarted;

        public ObservableCollection<ChatEntry> ChatEntries { get; private set; }
        public bool HasNoPlayersLookingToPlay { get; private set; }
        public CountdownViewModel BanCountdown { get; private set; }
        public ICommand SendMessageCommand { get; private set; }
        public ICommand FindMatchCommand { get; private set; }
        public ICommand CancelSearchCommand { get; private set; }
        public ICommand ChallengeCommand { get; private set; }
        public ICommand FriendRequestCommand { get; private set; }
        public ICommand AcceptChallengeCommand { get; private set; }
        public ICommand DeclineChallengeCommand { get; private set; }
        public ICommand CancelChallengeCommand { get; private set; }

        public IList<LobbyPlayerViewModel> LookingToPlay
        {
            get { return _lobby.LookingToPlay; }
        }

        public IList<MatchPair> MatchesInProgress
        {
            get { return _lobby.MatchesInProgress; }
        }

        public int PlayersInMatchCount
        {
            get { return _lobby.PlayersInMatchCount; }
        }

        public IList<LobbyPlayerViewModel> Unavailable
        {
            get { return _lobby.Unavailable; }
        }

        public IList<LobbyPlayerViewModel> FriendsOnline
        {
            get { return _lobby.FriendsOnline; }
        }

        public IList<MatchPair> FriendsInMatch
        {
            get { return _lobby.FriendsInMatch; }
        }

        public int FriendsInMatchCount
        {
            get { return _lobby.FriendsInMatchCount; }
        }

        public IList<LobbyPlayerViewModel> FriendsUnavailable
        {
            get { return _lobby.FriendsUnavailable; }
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
            get { return !_isChatBlocked; }
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

        // Only the messages of other players can be reported, so the notices and the own messages open no menu.
        public bool CanReport(ChatEntry entry)
        {
            ChatMessage message = entry as ChatMessage;
            return (message != null) && (message.AuthorName != _lobby.PlayerName);
        }

        public PlayerProfile GetOwnProfile()
        {
            return _dataService.GetOwnProfile();
        }

        public PlayerProfile GetPlayerProfile(LobbyPlayerViewModel player)
        {
            return _dataService.GetPlayerProfile(player);
        }

        // Changes made in the profile are copied back to the row it was opened from so the panel and the profile never disagree.
        public void ApplyProfileChanges(LobbyPlayerViewModel player, PlayerProfile profile)
        {
            player.HasPendingRequest = profile.HasPendingRequest;
            if (player.IsFriend && !profile.IsFriend)
            {
                RemoveFriend(player.Username);
            }
        }

        // The friend leaves the friends tab, as CU-16 Post-2 and Post-3 require.
        // Every other row that lists the player offers a friend request again instead of the removal.
        public void RemoveFriend(string username)
        {
            RemoveByName(_lobby.FriendsOnline, username);
            RemoveByName(_lobby.FriendsUnavailable, username);
            IEnumerable<LobbyPlayerViewModel> otherRows = _lobby.LookingToPlay
                .Concat(_lobby.Online)
                .Concat(_lobby.Unavailable)
                .Where(candidate => candidate.Username == username);
            foreach (LobbyPlayerViewModel player in otherRows)
            {
                player.IsFriend = false;
            }

            HasNoFriends = HasNoFriendsIn(_lobby);
        }

        // An empty message is not sent and shows no error (CU-17 FA-03).
        // Until the server relays messages to the lobby, the screen shows its own message so the chat flow can be walked through.
        private void SendMessage()
        {
            string content = _messageDraft.Trim();
            if (CanWriteMessage && (content.Length > 0))
            {
                ChatMessage message = new ChatMessage();
                message.AuthorName = _lobby.PlayerName;
                message.Content = content;
                message.SentAt = DateTime.UtcNow;
                ChatEntries.Add(message);
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

        private void Challenge(object parameter)
        {
            LobbyPlayerViewModel player = parameter as LobbyPlayerViewModel;
            if (player != null)
            {
                ChallengeNotice notice = new ChallengeNotice();
                notice.Kind = ChallengeNoticeKind.Sent;
                notice.PlayerName = player.Username;
                notice.SentAt = DateTime.UtcNow;
                ChatEntries.Add(notice);
            }
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

        // Accepting starts the match, and the table opens over the lobby until the match ends (CU-23).
        private void AcceptChallenge(object parameter)
        {
            RemoveChatEntry(parameter);
            MatchStarted?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveChatEntry(object parameter)
        {
            ChatEntries.Remove(parameter as ChatEntry);
        }

        private static void RemoveByName(IList<LobbyPlayerViewModel> players, string username)
        {
            LobbyPlayerViewModel player = players.FirstOrDefault(candidate => candidate.Username == username);
            if (player != null)
            {
                players.Remove(player);
            }
        }

        private static bool HasNoFriendsIn(LobbySnapshot lobby)
        {
            return (lobby.FriendsOnline.Count == 0)
                && (lobby.FriendsInMatch.Count == 0)
                && (lobby.FriendsUnavailable.Count == 0);
        }
    }
}
