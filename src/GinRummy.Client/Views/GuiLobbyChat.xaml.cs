using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using GinRummy.Client.Controls;
using GinRummy.Client.Models;
using GinRummy.Client.Services;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Lobby of a registered player (P11). Hosts the chat of CU-17, the search for a match of
    /// CU-22 and the challenges of CU-23, CU-24 and CU-25, and holds the panel of players from
    /// which CU-10, CU-11, CU-12, CU-16 and CU-27 start.
    /// </summary>
    public partial class GuiLobbyChat : GuiWindowBase
    {
        private const int TimerIntervalSeconds = 1;
        private const string TimeRemainingKey = "Sanctions_LblTimeRemaining";

        private readonly ObservableCollection<ChatEntry> _chatEntries;
        private readonly DispatcherTimer _countdownTimer;
        private readonly LobbySnapshot _lobby;
        private TimeSpan _remainingBanTime;
        private LobbyPlayerViewModel _friendToRemove;

        public GuiLobbyChat()
        {
            InitializeComponent();
            SampleDataService dataService = new SampleDataService();
            _lobby = dataService.GetLobby();
            _chatEntries = new ObservableCollection<ChatEntry>(_lobby.ChatEntries);
            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(TimerIntervalSeconds);
            _countdownTimer.Tick += OnCountdownTick;
            DataContext = _lobby;
            lstMessages.ItemsSource = _chatEntries;
            lblEmptyNoPlayers.Visibility = VisibilityCommon.FromCondition(_lobby.LookingToPlay.Count == 0);
            lblEmptyState.Visibility = VisibilityCommon.FromCondition(HasNoFriends(_lobby));
            Loaded += OnScreenLoaded;
            Closed += OnScreenClosed;
        }

        // Blocks the chat of a player whose sanction restricts it (CU-17 EX-08).
        public void ShowChatBlocked(string reasonName, TimeSpan remainingTime)
        {
            lblReasonBan.Text = reasonName;
            _remainingBanTime = remainingTime;
            lblErrorMessage.Visibility = Visibility.Visible;
            txtMessage.IsEnabled = false;
            btnSend.IsEnabled = false;
            _countdownTimer.Start();
            RefreshFormattedText();
        }

        protected override void RefreshFormattedText()
        {
            if (lblTimeRemaining != null)
            {
                lblTimeRemaining.Text = Localization.Format(
                    TimeRemainingKey,
                    DurationCommon.ToClock(_remainingBanTime, Localization.CurrentCulture));
            }
        }

        private void OnScreenLoaded(object sender, RoutedEventArgs e)
        {
            ScrollToLatestEntry();
        }

        private void OnCountdownTick(object sender, EventArgs e)
        {
            if (_remainingBanTime > TimeSpan.Zero)
            {
                _remainingBanTime = _remainingBanTime.Subtract(TimeSpan.FromSeconds(TimerIntervalSeconds));
            }

            RefreshFormattedText();
        }

        private void OnSendClick(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void OnMessageKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void OnFindMatchClick(object sender, RoutedEventArgs e)
        {
            // The search stays in the state of CU-22 FA-03, the one the prototype draws, until the server pairs the players.
            lblSearchingMatch.Visibility = Visibility.Visible;
            btnFindMatch.IsEnabled = false;
        }

        private void OnCancelSearchClick(object sender, RoutedEventArgs e)
        {
            lblSearchingMatch.Visibility = Visibility.Collapsed;
            btnFindMatch.IsEnabled = true;
        }

        private void OnChallengeClick(object sender, RoutedEventArgs e)
        {
            LobbyPlayerViewModel player = GetMenuPlayer(sender);
            if (player != null)
            {
                ChallengeNotice notice = new ChallengeNotice();
                notice.Kind = ChallengeNoticeKind.Sent;
                notice.PlayerName = player.Username;
                notice.SentAt = DateTime.Now;
                AddChatEntry(notice);
            }
        }

        private void OnPlayerProfileClick(object sender, RoutedEventArgs e)
        {
            LobbyPlayerViewModel player = GetMenuPlayer(sender);
            if (player != null)
            {
                SampleDataService dataService = new SampleDataService();
                PlayerProfile profile = dataService.GetPlayerProfile(player);
                ShowProfile(profile);

                // Changes made in the profile are copied back to the row it was opened from so the panel and the profile never disagree.
                player.HasPendingRequest = profile.HasPendingRequest;
                if (player.IsFriend && !profile.IsFriend)
                {
                    RemoveFriend(player.Username);
                }
            }
        }

        private void OnFriendRequestClick(object sender, RoutedEventArgs e)
        {
            // The option turns into the pending request mark so a second request cannot be sent while the first one waits (CU-12 step 8).
            LobbyPlayerViewModel player = GetMenuPlayer(sender);
            if (player != null)
            {
                player.HasPendingRequest = true;
            }
        }

        private void OnRemoveFriendClick(object sender, RoutedEventArgs e)
        {
            _friendToRemove = GetMenuPlayer(sender);
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.RemoveFriend);
            confirmDialog.Closed += OnRemoveFriendConfirmClosed;
            ShowModal(confirmDialog);
        }

        private void OnRemoveFriendConfirmClosed(object sender, EventArgs e)
        {
            if ((_friendToRemove != null) && ((GuiConfirmDialog)sender).IsConfirmed)
            {
                RemoveFriend(_friendToRemove.Username);
            }

            _friendToRemove = null;
        }

        private void OnMessageMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Only the messages of other players can be reported, so the notices and the own messages open no menu.
            ChatMessage message = ((FrameworkElement)sender).DataContext as ChatMessage;
            e.Handled = (message == null) || (message.AuthorName == _lobby.PlayerName);
        }

        private void OnCancelChallengeClick(object sender, RoutedEventArgs e)
        {
            RemoveChatEntry(sender);
        }

        private void OnAcceptChallengeClick(object sender, RoutedEventArgs e)
        {
            // Accepting starts the match, and the table opens over the lobby until the match ends (CU-23).
            RemoveChatEntry(sender);
            OpenOver(new GuiGameTable());
        }

        private void OnDeclineChallengeClick(object sender, RoutedEventArgs e)
        {
            RemoveChatEntry(sender);
        }

        private void OnNotificationsClick(object sender, RoutedEventArgs e)
        {
            GuiNotifications notifications = new GuiNotifications();
            ShowModal(notifications);
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            GuiProfilePanel profilePanel = new GuiProfilePanel();
            ShowModal(profilePanel);
        }

        private void OnMyProfileClick(object sender, RoutedEventArgs e)
        {
            ShowOwnProfile();
        }

        private void OnProfileClick(object sender, RoutedEventArgs e)
        {
            ShowOwnProfile();
        }

        private void OnRankingsClick(object sender, RoutedEventArgs e)
        {
            GuiRankings rankings = new GuiRankings();
            ShowModal(rankings);
        }

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            GuiHouseRules houseRules = new GuiHouseRules();
            ShowModal(houseRules);
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _countdownTimer.Stop();
            _countdownTimer.Tick -= OnCountdownTick;
            Loaded -= OnScreenLoaded;
            Closed -= OnScreenClosed;
        }

        private void SendMessage()
        {
            string content = txtMessage.Text.Trim();

            // An empty message is not sent and shows no error (CU-17 FA-03).
            // Until the server relays messages to the lobby, the screen shows its own message so the chat flow can be walked through.
            if (content.Length > 0)
            {
                ChatMessage message = new ChatMessage();
                message.AuthorName = _lobby.PlayerName;
                message.Content = content;
                message.SentAt = DateTime.Now;
                AddChatEntry(message);
                txtMessage.Clear();
            }
        }

        private void AddChatEntry(ChatEntry entry)
        {
            _chatEntries.Add(entry);
            ScrollToLatestEntry();
        }

        private void RemoveChatEntry(object sender)
        {
            ChatEntry entry = ((FrameworkElement)sender).DataContext as ChatEntry;
            _chatEntries.Remove(entry);
        }

        private void ShowOwnProfile()
        {
            // The own profile reuses the profile screen of any other player, opened from the header (CU-27 FA-02).
            SampleDataService dataService = new SampleDataService();
            ShowProfile(dataService.GetOwnProfile());
        }

        private void ShowProfile(PlayerProfile profile)
        {
            GuiPlayerProfile playerProfile = new GuiPlayerProfile(profile);
            ShowModal(playerProfile);
        }

        private void RemoveFriend(string username)
        {
            // The friend leaves the friends tab, as CU-16 Post-2 and Post-3 require.
            // Every other row that lists the player offers a friend request again instead of the removal.
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

            lblEmptyState.Visibility = VisibilityCommon.FromCondition(HasNoFriends(_lobby));
        }

        private void ScrollToLatestEntry()
        {
            if (_chatEntries.Count > 0)
            {
                OverlayInsetCommon.ScrollToEnd(lstMessages);
            }
        }

        private static LobbyPlayerViewModel GetMenuPlayer(object sender)
        {
            return ((FrameworkElement)sender).DataContext as LobbyPlayerViewModel;
        }

        private static void RemoveByName(IList<LobbyPlayerViewModel> players, string username)
        {
            LobbyPlayerViewModel player = players.FirstOrDefault(candidate => candidate.Username == username);
            if (player != null)
            {
                players.Remove(player);
            }
        }

        private static bool HasNoFriends(LobbySnapshot lobby)
        {
            return (lobby.FriendsOnline.Count == 0)
                && (lobby.FriendsInMatch.Count == 0)
                && (lobby.FriendsUnavailable.Count == 0);
        }
    }
}
