using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

using GinRummy.Client.ChatMessageServiceReference;
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
        private readonly LobbyChatViewModel _viewModel;
        private readonly LobbyChallengeViewModel _challengeViewModel;
        private readonly PlayerProfile _ownProfile;
        private readonly PlayerProfileViewModel _ownProfileViewModel;
        private readonly FriendshipConnection _friendshipConnection;
        private readonly LobbyFriendshipViewModel _friendshipViewModel;
        private LobbyPlayerViewModel _friendToRemove;

        public GuiLobbyChat()
        {
            InitializeComponent();
            _viewModel = ((App)Application.Current).CreateLobbyChatViewModel();
            _viewModel.PresenceUnavailable += OnPresenceUnavailable;
            _viewModel.ChatEntries.CollectionChanged += OnChatEntriesChanged;
            DataContext = _viewModel;
            _challengeViewModel = ((App)Application.Current).CreateLobbyChallengeViewModel();
            _challengeViewModel.RoundStarted += OnRoundStarted;
            _challengeViewModel.SessionExpired += OnSessionExpired;
            pnlChallenges.DataContext = _challengeViewModel;
            _friendshipConnection = ((App)Application.Current).CreateFriendshipConnection();
            _friendshipViewModel = ((App)Application.Current).CreateLobbyFriendshipViewModel(_friendshipConnection);
            pnlFriendshipNotice.DataContext = _friendshipViewModel;

            // The rail, the profile screen and the panel share one own profile, so a new picture shows on all of them (CU-28).
            _ownProfile = _viewModel.GetOwnProfile();
            _ownProfileViewModel = ((App)Application.Current).CreatePlayerProfileViewModel(_ownProfile);
            imgMyProfilePicture.DataContext = _ownProfileViewModel;
            Loaded += OnScreenLoaded;
            Closed += OnScreenClosed;
        }

        // The follow-up of the players starts once the window is on screen, because a failure opens the error dialog over it.
        private void OnScreenLoaded(object sender, RoutedEventArgs e)
        {
            ScrollToLatestEntry();
            _viewModel.StartPresence();
            _viewModel.StartChat();
            _challengeViewModel.Start();
            _friendshipViewModel.Start();
            _ownProfileViewModel.LoadProfile(((App)Application.Current).SignedInPlayer.AccessToken);
        }

        private void OnChatEntriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ScrollToLatestEntry();
            }
        }

        private void OnPresenceUnavailable(object sender, EventArgs e)
        {
            ShowModal(new GuiErrorDialog(ErrorDialogKind.ServiceUnavailable));
        }

        // A context menu lives outside the visual tree of the window, so its items cannot bind to the commands of the lobby.
        // Each item hands the row it was opened on to the command instead.
        private void OnChallengeClick(object sender, RoutedEventArgs e)
        {
            LobbyPlayerViewModel player = GetMenuPlayer(sender);
            if (player != null)
            {
                _challengeViewModel.Challenge(player);
            }
        }

        private void OnRoundStarted(object sender, GameTableViewModel table)
        {
            OpenOver(new GuiGameTable(table));
        }

        private void OnSessionExpired(object sender, EventArgs e)
        {
            GuiErrorDialog errorDialog = new GuiErrorDialog(ErrorDialogKind.SessionExpired);
            errorDialog.Closed += OnSessionExpiredDialogClosed;
            ShowModal(errorDialog);
        }

        private void OnSessionExpiredDialogClosed(object sender, EventArgs e)
        {
            Close();
            ((App)Application.Current).ForgetSignedInPlayer();
        }

        private void OnFriendRequestClick(object sender, RoutedEventArgs e)
        {
            LobbyPlayerViewModel player = GetMenuPlayer(sender);
            if (player != null)
            {
                _friendshipViewModel.SendFriendRequest(player);
            }
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
            PlayerMessage message = ((FrameworkElement)sender).DataContext as PlayerMessage;
            e.Handled = !_viewModel.CanReport(message);
        }

        private void OnNotificationsClick(object sender, RoutedEventArgs e)
        {
            GuiNotifications notifications = new GuiNotifications(_friendshipConnection);
            ShowModal(notifications);
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            GuiProfilePanel profilePanel = new GuiProfilePanel(_ownProfile);
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
            _viewModel.StopChat();
            _challengeViewModel.Stop();
            _friendshipViewModel.Stop();
            _challengeViewModel.RoundStarted -= OnRoundStarted;
            _challengeViewModel.SessionExpired -= OnSessionExpired;
            _viewModel.PresenceUnavailable -= OnPresenceUnavailable;
            _viewModel.ChatEntries.CollectionChanged -= OnChatEntriesChanged;
            Loaded -= OnScreenLoaded;
            Closed -= OnScreenClosed;
        }

        private void ShowOwnProfile()
        {
            GuiPlayerProfile playerProfile = new GuiPlayerProfile(_ownProfile);
            playerProfile.Closed += OnOwnProfileClosed;
            ShowModal(playerProfile);
        }

        private void OnOwnProfileClosed(object sender, EventArgs e)
        {
            _ownProfileViewModel.RefreshProfile();
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
