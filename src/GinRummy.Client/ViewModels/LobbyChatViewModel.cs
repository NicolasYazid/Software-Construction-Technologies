using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
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
        private const string PlayerNotFoundNoticeKey = "Error_SocialPlayerNotFound";
        private const string OwnTagNoticeKey = "Search_OwnTagNotice";
        private const string DisallowedCharactersNoticeKey = "Error_ValDisallowedCharacters";

        private readonly PlayerPresenceMonitor _presenceMonitor;
        private readonly ChatMessageConnection _chatConnection;
        private readonly LogInOutcome _signedInPlayer;
        private string _messageDraft;
        private bool _isSearchingMatch;
        private bool _isChatBlocked;
        private bool _isChatUnavailable;
        private bool _hasNoFriends;
        private string _banReasonName;
        private string _searchText;
        private string _searchNoticeKey;

        // The lobby of a player and the one of a guest show the same panel of players, so both screens use this view model.
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
            SortPlayersByRank();
            BanCountdown = new CountdownViewModel(TimeSpan.Zero);
            SendMessageCommand = new RelayCommand(SendMessage);
            FindMatchCommand = new RelayCommand(FindMatch);
            CancelSearchCommand = new RelayCommand(CancelSearch);
        }

        public event EventHandler PresenceUnavailable;

        public ObservableCollection<PlayerMessage> ChatEntries { get; private set; }
        public bool HasNoPlayersLookingToPlay { get; private set; }
        public CountdownViewModel BanCountdown { get; private set; }
        public ICommand SendMessageCommand { get; private set; }
        public ICommand FindMatchCommand { get; private set; }
        public ICommand CancelSearchCommand { get; private set; }
        public IList<LobbyPlayerViewModel> LookingToPlay { get; private set; }
        public IList<LobbyPlayerViewModel> Online { get; private set; }
        public IList<MatchPair> MatchesInProgress { get; private set; }
        public int PlayersInMatchCount { get; private set; }
        public IList<LobbyPlayerViewModel> Unavailable { get; private set; }
        public IList<LobbyPlayerViewModel> FriendsOnline { get; private set; }
        public IList<MatchPair> FriendsInMatch { get; private set; }
        public int FriendsInMatchCount { get; private set; }
        public IList<LobbyPlayerViewModel> FriendsUnavailable { get; private set; }

        public string PlayerName
        {
            get { return _signedInPlayer.Username; }
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

        // Only the tag is needed, because the profile screen reads everything else from the server (CU-27 FA-02).
        public PlayerProfile GetOwnProfile()
        {
            PlayerProfile profile = new PlayerProfile();
            profile.PublicTag = _signedInPlayer.PublicTag;
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
            if (player.PublicTag.HasValue)
            {
                profile.PublicTag = player.PublicTag.Value;
            }

            profile.RankName = player.RankName;
            profile.SocialLinks = new List<SocialLink>();
            profile.IsFriend = player.IsFriend;
            profile.IsGuest = player.IsGuest;

            return profile;
        }

        public void ApplyProfileChanges(LobbyPlayerViewModel player, PlayerProfile profile)
        {
            if (profile.IsRemoved)
            {
                RemovePlayer(player);
            }
            else if (player.IsFriend && !profile.IsFriend)
            {
                RemoveFriend(player.Username);
            }
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

        public void AddFriend(int publicTag)
        {
            LobbyPlayerViewModel onlinePlayer = FindByPublicTag(Online, publicTag);
            LobbyPlayerViewModel unavailablePlayer = FindByPublicTag(Unavailable, publicTag);
            bool isListedAsFriend = (FindByPublicTag(FriendsOnline, publicTag) != null)
                || (FindByPublicTag(FriendsUnavailable, publicTag) != null);
            if (!isListedAsFriend && (onlinePlayer != null))
            {
                FriendsOnline.Add(CopyAsFriend(onlinePlayer));
            }
            else if (!isListedAsFriend && (unavailablePlayer != null))
            {
                FriendsUnavailable.Add(CopyAsFriend(unavailablePlayer));
            }

            UpdateRelationOfPlayer(publicTag, true);
            HasNoFriends = HasNoFriendsListed();
        }

        public void ClearPendingRequest(int publicTag)
        {
            UpdateRelationOfPlayer(publicTag, false);
        }

        private void SendMessage()
        {
            string content = _messageDraft.Trim();
            if (CanWriteMessage && (content.Length > 0))
            {
                _chatConnection.SendLobbyMessage(_signedInPlayer.AccessToken, content);
                MessageDraft = string.Empty;
            }
        }

        private void FindMatch()
        {
            IsSearchingMatch = true;
        }

        private void CancelSearch()
        {
            IsSearchingMatch = false;
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
            RemovePlayerRow(Online, presence.PublicTag, presence.GuestSessionId);
            RemovePlayerRow(Unavailable, presence.PublicTag, presence.GuestSessionId);
            RemovePlayerRow(FriendsOnline, presence.PublicTag, presence.GuestSessionId);
            RemovePlayerRow(FriendsUnavailable, presence.PublicTag, presence.GuestSessionId);
            if (presence.Status == PresenceStatus.Online)
            {
                AddPlayer(Online, FriendsOnline, presence);
            }
            else if (!presence.IsGuest)
            {
                // A guest has no account to come back to, so it leaves the panel instead of becoming unavailable.
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

        private void RemovePlayer(LobbyPlayerViewModel player)
        {
            RemovePlayerRow(Online, player.PublicTag, player.GuestSessionId);
            RemovePlayerRow(Unavailable, player.PublicTag, player.GuestSessionId);
            RemovePlayerRow(FriendsOnline, player.PublicTag, player.GuestSessionId);
            RemovePlayerRow(FriendsUnavailable, player.PublicTag, player.GuestSessionId);
            HasNoFriends = HasNoFriendsListed();
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
            else if (typedTag == _signedInPlayer.PublicTag.ToString(CultureInfo.InvariantCulture))
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
                LookingToPlay,
                Online,
                Unavailable,
                FriendsOnline,
                FriendsUnavailable
            };

            return playerGroups;
        }

        // The tag is compared from its start, so the row appears while the player is still typing the digits.
        private static bool TagStartsWith(LobbyPlayerViewModel player, string typedTag)
        {
            return player.PublicTag.HasValue
                && player.PublicTag.Value.ToString(CultureInfo.InvariantCulture).StartsWith(typedTag, StringComparison.Ordinal);
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

        // A player with an account has a tag and no guest session, and a guest has the opposite, so the two never match each other.
        private static void RemovePlayerRow(IList<LobbyPlayerViewModel> players, int? publicTag, int? guestSessionId)
        {
            LobbyPlayerViewModel player = players.FirstOrDefault(
                candidate => (candidate.PublicTag == publicTag) && (candidate.GuestSessionId == guestSessionId));
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
            player.GuestSessionId = presence.GuestSessionId;
            player.RankName = presence.RankName;
            player.Score = presence.Score;
            player.IsFriend = presence.IsFriend;
            player.IsGuest = presence.IsGuest;

            return player;
        }

        private void UpdateRelationOfPlayer(int publicTag, bool isFriend)
        {
            List<IList<LobbyPlayerViewModel>> groupsOfEveryone = new List<IList<LobbyPlayerViewModel>>
            {
                LookingToPlay,
                Online,
                Unavailable
            };
            foreach (IList<LobbyPlayerViewModel> players in groupsOfEveryone)
            {
                LobbyPlayerViewModel player = FindByPublicTag(players, publicTag);
                if (player != null)
                {
                    player.IsFriend = isFriend;
                    player.HasPendingRequest = false;
                }
            }
        }

        private static LobbyPlayerViewModel FindByPublicTag(IList<LobbyPlayerViewModel> players, int publicTag)
        {
            LobbyPlayerViewModel foundPlayer = null;
            foreach (LobbyPlayerViewModel candidate in players)
            {
                if (candidate.PublicTag == publicTag)
                {
                    foundPlayer = candidate;
                }
            }

            return foundPlayer;
        }

        private static LobbyPlayerViewModel CopyAsFriend(LobbyPlayerViewModel player)
        {
            LobbyPlayerViewModel friend = new LobbyPlayerViewModel();
            friend.Username = player.Username;
            friend.PublicTag = player.PublicTag;
            friend.GuestSessionId = player.GuestSessionId;
            friend.RankName = player.RankName;
            friend.Score = player.Score;
            friend.IsFriend = true;
            friend.IsGuest = player.IsGuest;

            return friend;
        }
    }
}
