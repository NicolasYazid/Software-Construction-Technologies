using System;
using System.Windows;

using GinRummy.Client.Controls;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Lobby of a guest (P16). Serves CU-02 FA-01 and the guest flows of CU-11 FA-02, CU-17
    /// FA-02 and CU-22 FA-01: the guest reads the chat and looks for a match, but can neither
    /// write nor have friends until it creates an account (CU-01 FA-05).
    /// </summary>
    public partial class GuiGuestLobby : GuiWindowBase
    {
        private readonly GuestLobbyViewModel _viewModel;

        public GuiGuestLobby()
        {
            InitializeComponent();
            // The composition root builds the lobby: when a public tag was passed at startup it enables the
            // live friends tab. Enabling the tab for a guest and taking the identity from an argument are
            // temporary shortcuts until CU-02 and the authenticated lobby exist.
            _viewModel = ((App)Application.Current).CreateGuestLobbyViewModel();
            DataContext = _viewModel;
            Loaded += OnScreenLoaded;
            Closed += OnScreenClosed;
        }

        private void OnScreenLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.StartPresence();
            if (_viewModel.ChatEntries.Count > 0)
            {
                OverlayInsetCommon.ScrollToEnd(lstMessages);
            }
        }

        private void OnLogInClick(object sender, RoutedEventArgs e)
        {
            ReturnToMenuWith(new GuiLogIn());
        }

        private void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            ReturnToMenuWith(new GuiSignUp());
        }

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            GuiHouseRules houseRules = new GuiHouseRules();
            ShowModal(houseRules);
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.StopPresence();
            Loaded -= OnScreenLoaded;
            Closed -= OnScreenClosed;
        }
    }
}
