using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using GinRummy.Client.Models;
using GinRummy.Client.Services;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Lobby of a registered player (P11). Hosts the chat of CU-17, the search for a match of
    /// CU-22 and the challenges of CU-23, CU-24 and CU-25, and holds the panel of players from
    /// which CU-10, CU-11, CU-12, CU-16, CU-18 and CU-27 start.
    /// </summary>
    public partial class GuiLobbyChat : GuiWindowBase
    {
        private const int TimerIntervalSeconds = 1;
        private const string TimeRemainingKey = "Sanctions_LblTimeRemaining";

        private readonly ObservableCollection<ChatEntryDto> _chatEntries;
        private readonly DispatcherTimer _countdownTimer;
        private readonly LobbySnapshotDto _lobby;
        private TimeSpan _remainingBanTime;

        /// <summary>
        /// Builds the lobby with the state the player finds when entering.
        /// </summary>
        public GuiLobbyChat()
        {
            InitializeComponent();
            SampleDataService dataService = new SampleDataService();
            _lobby = dataService.GetLobby();
            _chatEntries = new ObservableCollection<ChatEntryDto>(_lobby.ChatEntries);
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

        /// <summary>
        /// Blocks the chat of a player whose sanction restricts it (CU-17 EX-08). The field
        /// stays disabled and the remaining time counts down while the ban is in force.
        /// </summary>
        /// <param name="reasonName">Name of the reason of the ban, already in the active
        /// language.</param>
        /// <param name="remainingTime">Time left until the ban expires.</param>
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

        /// <summary>
        /// Rebuilds the remaining time of the block of the chat, which carries a placeholder.
        /// </summary>
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
            // Until the server pairs the players, the search stays in the state of CU-22 FA-03,
            // which is the one the prototype draws.
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
            LobbyPlayerDto player = GetMenuPlayer(sender);
            if (player != null)
            {
                ChallengeNoticeDto notice = new ChallengeNoticeDto();
                notice.Kind = ChallengeNoticeKind.Sent;
                notice.PlayerName = player.Username;
                notice.SentAt = DateTime.Now;
                AddChatEntry(notice);
            }
        }

        private void OnPlayerProfileClick(object sender, RoutedEventArgs e)
        {
            LobbyPlayerDto player = GetMenuPlayer(sender);
            if (player != null)
            {
                SampleDataService dataService = new SampleDataService();
                PlayerProfileDto profile = dataService.GetPlayerProfile(player);
                ShowProfile(profile);

                // What the player did from the profile carries back to the row it was opened
                // from, so the panel and the profile never disagree.
                player.HasPendingRequest = profile.HasPendingRequest;
                if (player.IsFriend && !profile.IsFriend)
                {
                    RemoveFriend(player.Username);
                }
            }
        }

        private void OnFriendRequestClick(object sender, RoutedEventArgs e)
        {
            // Once the server creates the request, the option turns into the mark of a pending
            // request, so a second one cannot leave while the first waits (CU-12 step 8).
            LobbyPlayerDto player = GetMenuPlayer(sender);
            if (player != null)
            {
                player.HasPendingRequest = true;
            }
        }

        private void OnReportClick(object sender, RoutedEventArgs e)
        {
            LobbyPlayerDto player = GetMenuPlayer(sender);
            if (player != null)
            {
                GuiReportPlayer reportPlayer = new GuiReportPlayer(player.Username);
                ShowModal(reportPlayer);
            }
        }

        private void OnRemoveFriendClick(object sender, RoutedEventArgs e)
        {
            LobbyPlayerDto player = GetMenuPlayer(sender);
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.RemoveFriend);
            confirmDialog.Closed += (source, arguments) =>
            {
                if (player != null && confirmDialog.IsConfirmed)
                {
                    RemoveFriend(player.Username);
                }
            };
            ShowModal(confirmDialog);
        }

        private void OnCancelChallengeClick(object sender, RoutedEventArgs e)
        {
            RemoveChatEntry(sender);
        }

        private void OnAcceptChallengeClick(object sender, RoutedEventArgs e)
        {
            // Accepting starts the match, and the table opens over the lobby until the match
            // ends (CU-23).
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

            // An empty message is not sent and shows no error (CU-17 FA-03). The server relays
            // every other one to the lobby; until it answers, the screen shows its own message
            // so that the chat can be walked.
            if (content.Length > 0)
            {
                ChatMessageDto message = new ChatMessageDto();
                message.AuthorName = _lobby.PlayerName;
                message.Content = content;
                message.SentAt = DateTime.Now;
                AddChatEntry(message);
                txtMessage.Clear();
            }
        }

        private void AddChatEntry(ChatEntryDto entry)
        {
            _chatEntries.Add(entry);
            ScrollToLatestEntry();
        }

        private void RemoveChatEntry(object sender)
        {
            ChatEntryDto entry = ((FrameworkElement)sender).DataContext as ChatEntryDto;
            _chatEntries.Remove(entry);
        }

        private void ShowOwnProfile()
        {
            // The own profile is the same screen as the one of any other player, opened from
            // the header (CU-27 FA-02).
            SampleDataService dataService = new SampleDataService();
            ShowProfile(dataService.GetOwnProfile());
        }

        private void ShowProfile(PlayerProfileDto profile)
        {
            GuiPlayerProfile playerProfile = new GuiPlayerProfile(profile);
            ShowModal(playerProfile);
        }

        private void RemoveFriend(string username)
        {
            // The friend leaves the friends tab, and wherever else the panel lists it, its menu
            // stops offering to remove it and offers a request again (CU-16 Post-2 and Post-3).
            RemoveByName(_lobby.FriendsOnline, username);
            RemoveByName(_lobby.FriendsUnavailable, username);
            IEnumerable<LobbyPlayerDto> otherRows = _lobby.LookingToPlay
                .Concat(_lobby.Online)
                .Concat(_lobby.Unavailable)
                .Where(candidate => candidate.Username == username);
            foreach (LobbyPlayerDto player in otherRows)
            {
                player.IsFriend = false;
            }

            lblEmptyState.Visibility = VisibilityCommon.FromCondition(HasNoFriends(_lobby));
        }

        private void ScrollToLatestEntry()
        {
            if (_chatEntries.Count > 0)
            {
                lstMessages.ScrollIntoView(_chatEntries[_chatEntries.Count - 1]);
            }
        }

        private static LobbyPlayerDto GetMenuPlayer(object sender)
        {
            return ((FrameworkElement)sender).DataContext as LobbyPlayerDto;
        }

        private static void RemoveByName(IList<LobbyPlayerDto> players, string username)
        {
            LobbyPlayerDto player = players.FirstOrDefault(candidate => candidate.Username == username);
            if (player != null)
            {
                players.Remove(player);
            }
        }

        private static bool HasNoFriends(LobbySnapshotDto lobby)
        {
            return lobby.FriendsOnline.Count == 0
                && lobby.FriendsInMatch.Count == 0
                && lobby.FriendsUnavailable.Count == 0;
        }
    }
}
