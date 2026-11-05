using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.PlayerPresenceServiceReference;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class LobbyChatViewModel : ViewModelBase
    {
        private const string PlayerNotFoundNoticeKey = "Error_SocialPlayerNotFound";
        private const string OwnTagNoticeKey = "Search_OwnTagNotice";
        private const string DisallowedCharactersNoticeKey = "Error_ValDisallowedCharacters";

        private readonly PlayerPresenceMonitor _presenceMonitor;
        private readonly string _accessToken;
        private readonly LobbySnapshot _lobby;
        private string _messageDraft;
        private bool _isSearchingMatch;
        private bool _isChatBlocked;
        private bool _hasNoFriends;
        private string _banReasonName;
        private string _searchText;
        private string _searchNoticeKey;

        // The lobby of a player and the one of a guest show the same panel of players, so both screens use this view model.
        public LobbyChatViewModel(LobbySnapshot lobby, PlayerPresenceMonitor presenceMonitor, string accessToken)
        {
            _lobby = lobby ?? throw new ArgumentNullException(nameof(lobby));
            _presenceMonitor = presenceMonitor ?? throw new ArgumentNullException(nameof(presenceMonitor));
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            _messageDraft = string.Empty;
            ChatEntries = new ObservableCollection<ChatEntry>(_lobby.ChatEntries);
            HasNoPlayersLookingToPlay = _lobby.LookingToPlay.Count == 0;
            _hasNoFriends = HasNoFriendsIn(_lobby);
            SortPlayersByRank();
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
        public event EventHandler PresenceUnavailable;

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

        public string PlayerName
        {
            get { return _lobby.PlayerName; }
        }

        public IList<LobbyPlayerViewModel> LookingToPlay
        {
            get { return _lobby.LookingToPlay; }
        }

        public IList<LobbyPlayerViewModel> Online
        {
            get { return _lobby.Online; }
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

        // The player is not among the rows the server sends, so the own tag is kept apart for the search and the own profile.
        // The search uses it to answer a search for the own tag (CU-10 FA-02), and the profile to read it (CU-27 FA-02).
        public int OwnPublicTag { get; set; }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value ?? string.Empty;
                RaisePropertyChanged();
                FilterPlayersByTag();
            }
        }

        public string SearchNoticeKey
        {
            get { return _searchNoticeKey; }
            private set
            {
                _searchNoticeKey = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasSearchNotice));
            }
        }

        public bool HasSearchNotice
        {
            get { return _searchNoticeKey != null; }
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
            if (!_presenceMonitor.Start(_accessToken))
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

        public bool CanReport(ChatEntry entry)
        {
            ChatMessage message = entry as ChatMessage;
            return (message != null) && (message.AuthorName != _lobby.PlayerName);
        }

        // Only the tag is needed, because the profile screen reads everything else from the server (CU-27 FA-02).
        public PlayerProfile GetOwnProfile()
        {
            PlayerProfile profile = new PlayerProfile();
            profile.PublicTag = OwnPublicTag;
            profile.SocialLinks = new List<SocialLink>();
            profile.IsOwnProfile = true;

            return profile;
        }

        // The row already knows who the player is and how the two players are related.
        // The profile screen therefore asks the server only for the rest.
        public PlayerProfile GetPlayerProfile(LobbyPlayerViewModel player)
        {
            PlayerProfile profile = new PlayerProfile();
            profile.Username = player.Username;
            profile.PublicTag = player.PublicTag;
            profile.RankName = player.RankName;
            profile.SocialLinks = new List<SocialLink>();
            profile.IsFriend = player.IsFriend;
            profile.HasPendingRequest = player.HasPendingRequest;
            profile.IsGuest = player.IsGuest;

            return profile;
        }

        public void ApplyProfileChanges(LobbyPlayerViewModel player, PlayerProfile profile)
        {
            player.HasPendingRequest = profile.HasPendingRequest;
            if (profile.IsRemoved)
            {
                RemovePlayer(player.PublicTag);
            }
            else if (player.IsFriend && !profile.IsFriend)
            {
                RemoveFriend(player.Username);
            }
        }

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

        // Until the server relays messages to the lobby, the screen shows its own message so the chat flow can be walked through.
        private void SendMessage()
        {
            string content = _messageDraft.Trim();
            if (CanWriteMessage && (content.Length > 0))
            {
                ChatEntries.Add(new ChatMessage(_lobby.PlayerName, content, DateTime.UtcNow));
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
            LobbyPlayerViewModel player = (LobbyPlayerViewModel)parameter;
            ChatEntries.Add(new ChallengeNotice(ChallengeNoticeKind.Sent, player.Username, DateTime.UtcNow));
        }

        private void SendFriendRequest(object parameter)
        {
            LobbyPlayerViewModel player = (LobbyPlayerViewModel)parameter;
            player.HasPendingRequest = true;
        }

        private void AcceptChallenge(object parameter)
        {
            RemoveChatEntry(parameter);
            MatchStarted?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveChatEntry(object parameter)
        {
            ChatEntries.Remove((ChatEntry)parameter);
        }

        private void OnCurrentStatusReported(object sender, PlayerPresenceReport report)
        {
            ReplacePlayers(_lobby.Online, report.AllOnlinePlayers);
            ReplacePlayers(_lobby.Unavailable, report.AllOfflinePlayers);
            ReplacePlayers(_lobby.FriendsOnline, report.AllOnlineFriends);
            ReplacePlayers(_lobby.FriendsUnavailable, report.AllOfflineFriends);
            HasNoFriends = HasNoFriendsIn(_lobby);
        }

        // A player is listed in one status only, so its old row is removed before the new one is added.
        private void OnPlayerPresenceChanged(object sender, PlayerPresence presence)
        {
            RemoveByPublicTag(_lobby.Online, presence.PublicTag);
            RemoveByPublicTag(_lobby.Unavailable, presence.PublicTag);
            RemoveByPublicTag(_lobby.FriendsOnline, presence.PublicTag);
            RemoveByPublicTag(_lobby.FriendsUnavailable, presence.PublicTag);
            if (presence.Status == PresenceStatus.Online)
            {
                AddPlayer(_lobby.Online, _lobby.FriendsOnline, presence);
            }
            else if (!presence.IsGuest)
            {
                // A guest has no account to come back to, so it leaves the panel instead of becoming unavailable.
                AddPlayer(_lobby.Unavailable, _lobby.FriendsUnavailable, presence);
            }

            HasNoFriends = HasNoFriendsIn(_lobby);
        }

        // A player who no longer exists leaves every group of the panel (CU-27 EX-02).
        private void RemovePlayer(int publicTag)
        {
            RemoveByPublicTag(_lobby.Online, publicTag);
            RemoveByPublicTag(_lobby.Unavailable, publicTag);
            RemoveByPublicTag(_lobby.FriendsOnline, publicTag);
            RemoveByPublicTag(_lobby.FriendsUnavailable, publicTag);
            HasNoFriends = HasNoFriendsIn(_lobby);
        }

        private static void RemoveByName(IList<LobbyPlayerViewModel> players, string username)
        {
            LobbyPlayerViewModel player = players.FirstOrDefault(candidate => candidate.Username == username);
            if (player != null)
            {
                players.Remove(player);
            }
        }

        // Presence already sends every registered player with its relation, so the search filters the rows the panel has (CU-10).
        // The filter goes on the shared view of each group, so the everyone tab and the friends tab show the same result.
        private void FilterPlayersByTag()
        {
            string typedTag = _searchText.Trim();
            Predicate<object> tagFilter = null;
            if (typedTag.Length > 0)
            {
                tagFilter = candidate => TagStartsWith((LobbyPlayerViewModel)candidate, typedTag);
            }

            foreach (IList<LobbyPlayerViewModel> players in GetPlayerGroups())
            {
                CollectionViewSource.GetDefaultView(players).Filter = tagFilter;
            }

            string noticeKey = null;
            if (typedTag.Length > 0)
            {
                noticeKey = FindSearchNoticeKey(typedTag);
            }

            SearchNoticeKey = noticeKey;
        }

        // A higher score always falls in a higher rank, so the score orders the players from the highest rank to the lowest.
        // The order goes on the shared view of each group, so a player who arrives later takes its place in the list.
        private void SortPlayersByRank()
        {
            foreach (IList<LobbyPlayerViewModel> players in GetPlayerGroups())
            {
                ICollectionView playersView = CollectionViewSource.GetDefaultView(players);
                playersView.SortDescriptions.Add(new SortDescription(nameof(LobbyPlayerViewModel.Score), ListSortDirection.Descending));
                playersView.SortDescriptions.Add(new SortDescription(nameof(LobbyPlayerViewModel.Username), ListSortDirection.Ascending));
            }
        }

        private string FindSearchNoticeKey(string typedTag)
        {
            string noticeKey = null;
            if (!typedTag.All(char.IsDigit))
            {
                noticeKey = DisallowedCharactersNoticeKey;
            }
            else if (typedTag == OwnPublicTag.ToString(CultureInfo.InvariantCulture))
            {
                noticeKey = OwnTagNoticeKey;
            }
            else if (!GetPlayerGroups().SelectMany(players => players).Any(player => TagStartsWith(player, typedTag)))
            {
                noticeKey = PlayerNotFoundNoticeKey;
            }

            return noticeKey;
        }

        private IList<IList<LobbyPlayerViewModel>> GetPlayerGroups()
        {
            IList<IList<LobbyPlayerViewModel>> playerGroups = new List<IList<LobbyPlayerViewModel>>
            {
                _lobby.LookingToPlay,
                _lobby.Online,
                _lobby.Unavailable,
                _lobby.FriendsOnline,
                _lobby.FriendsUnavailable
            };

            return playerGroups;
        }

        // The tag is compared from its start, so the row appears while the player is still typing the digits.
        private static bool TagStartsWith(LobbyPlayerViewModel player, string typedTag)
        {
            return player.PublicTag.ToString(CultureInfo.InvariantCulture).StartsWith(typedTag, StringComparison.Ordinal);
        }

        private static bool HasNoFriendsIn(LobbySnapshot lobby)
        {
            return (lobby.FriendsOnline.Count == 0)
                && (lobby.FriendsInMatch.Count == 0)
                && (lobby.FriendsUnavailable.Count == 0);
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

        private static LobbyPlayerViewModel CreatePlayer(PlayerPresence presence)
        {
            LobbyPlayerViewModel player = new LobbyPlayerViewModel();
            player.Username = presence.Username;
            player.PublicTag = presence.PublicTag;
            player.RankName = presence.RankName;
            player.Score = presence.Score;
            player.IsFriend = presence.IsFriend;
            player.IsGuest = presence.IsGuest;

            return player;
        }
    }
}
