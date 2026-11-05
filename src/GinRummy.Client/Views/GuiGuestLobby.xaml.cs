using System;
using System.Windows;

using GinRummy.Client.Controls;
using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Lobby of a guest (P16). Serves CU-02 FA-01 and the guest flows of CU-11 FA-02, CU-17
    /// FA-02 and CU-22 FA-01: the guest follows the players, searches them by tag and reads the
    /// rankings, but can neither write nor have friends until it creates an account (CU-01 FA-05).
    /// </summary>
    public partial class GuiGuestLobby : GuiWindowBase
    {
        private readonly LobbyChatViewModel _viewModel;

        public GuiGuestLobby()
        {
            InitializeComponent();
            _viewModel = ((App)Application.Current).CreateGuestLobbyViewModel();
            _viewModel.PresenceUnavailable += OnPresenceUnavailable;
            DataContext = _viewModel;
            Loaded += OnScreenLoaded;
            Closed += OnScreenClosed;
        }

        // The follow-up of the players starts once the window is on screen, because a failure opens the error dialog over it.
        private void OnScreenLoaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel.ChatEntries.Count > 0)
            {
                OverlayInsetCommon.ScrollToEnd(lstMessages);
            }

            _viewModel.StartPresence();
        }

        private void OnPresenceUnavailable(object sender, EventArgs e)
        {
            ShowModal(new GuiErrorDialog(ErrorDialogKind.ServiceUnavailable));
        }

        private void OnLogInClick(object sender, RoutedEventArgs e)
        {
            ReturnToMenuWith(new GuiLogIn());
        }

        private void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            ReturnToMenuWith(new GuiSignUp(true));
        }

        private void OnRankingsClick(object sender, RoutedEventArgs e)
        {
            ShowModal(new GuiRankings());
        }

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            GuiHouseRules houseRules = new GuiHouseRules();
            ShowModal(houseRules);
        }

        // Leaving the lobby ends the presence subscription, so the other players stop seeing the guest.
        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.StopCountdown();
            _viewModel.StopPresence();
            _viewModel.PresenceUnavailable -= OnPresenceUnavailable;
            Loaded -= OnScreenLoaded;
            Closed -= OnScreenClosed;
            ((App)Application.Current).ForgetSignedInPlayer();
        }
    }
}
