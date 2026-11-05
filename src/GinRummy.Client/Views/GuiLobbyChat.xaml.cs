using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

using GinRummy.Client.Controls;
using GinRummy.Client.Models;
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
        private readonly LobbyChatViewModel _viewModel;
        private LobbyPlayerViewModel _friendToRemove;

        public GuiLobbyChat()
        {
            InitializeComponent();
            _viewModel = ((App)Application.Current).CreateLobbyChatViewModel();
            _viewModel.MatchStarted += OnMatchStarted;
            _viewModel.PresenceUnavailable += OnPresenceUnavailable;
            _viewModel.ChatEntries.CollectionChanged += OnChatEntriesChanged;
            _viewModel.OwnPublicTag = ((App)Application.Current).SignedInPlayer.PublicTag;
            DataContext = _viewModel;
            Loaded += OnScreenLoaded;
            Closed += OnScreenClosed;
        }

        // The follow-up of the players starts once the window is on screen, because a failure opens the error dialog over it.
        private void OnScreenLoaded(object sender, RoutedEventArgs e)
        {
            ScrollToLatestEntry();
            _viewModel.StartPresence();
        }

        private void OnChatEntriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ScrollToLatestEntry();
            }
        }

        private void OnMatchStarted(object sender, EventArgs e)
        {
            OpenOver(new GuiGameTable());
        }

        private void OnPresenceUnavailable(object sender, EventArgs e)
        {
            ShowModal(new GuiErrorDialog(ErrorDialogKind.ServiceUnavailable));
        }

        // A context menu lives outside the visual tree of the window, so its items cannot bind to the commands of the lobby.
        // Each item hands the row it was opened on to the command instead.
        private void OnChallengeClick(object sender, RoutedEventArgs e)
        {
            _viewModel.ChallengeCommand.Execute(GetMenuPlayer(sender));
        }

        private void OnFriendRequestClick(object sender, RoutedEventArgs e)
        {
            _viewModel.FriendRequestCommand.Execute(GetMenuPlayer(sender));
        }

        private void OnPlayerProfileClick(object sender, RoutedEventArgs e)
        {
            LobbyPlayerViewModel player = GetMenuPlayer(sender);
            if (player != null)
            {
                PlayerProfile profile = _viewModel.GetPlayerProfile(player);
                GuiPlayerProfile playerProfile = new GuiPlayerProfile(profile);
                playerProfile.Closed += (closedSender, closedArgs) => _viewModel.ApplyProfileChanges(player, profile);
                ShowModal(playerProfile);
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
                _viewModel.RemoveFriend(_friendToRemove.Username);
            }

            _friendToRemove = null;
        }

        private void OnMessageMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ChatEntry entry = ((FrameworkElement)sender).DataContext as ChatEntry;
            e.Handled = !_viewModel.CanReport(entry);
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
            _viewModel.StopCountdown();
            _viewModel.StopPresence();
            _viewModel.MatchStarted -= OnMatchStarted;
            _viewModel.PresenceUnavailable -= OnPresenceUnavailable;
            _viewModel.ChatEntries.CollectionChanged -= OnChatEntriesChanged;
            Loaded -= OnScreenLoaded;
            Closed -= OnScreenClosed;
        }

        private void ShowOwnProfile()
        {
            GuiPlayerProfile playerProfile = new GuiPlayerProfile(_viewModel.GetOwnProfile());
            ShowModal(playerProfile);
        }

        private void ScrollToLatestEntry()
        {
            if (_viewModel.ChatEntries.Count > 0)
            {
                OverlayInsetCommon.ScrollToEnd(lstMessages);
            }
        }

        private static LobbyPlayerViewModel GetMenuPlayer(object sender)
        {
            return ((FrameworkElement)sender).DataContext as LobbyPlayerViewModel;
        }
    }
}
