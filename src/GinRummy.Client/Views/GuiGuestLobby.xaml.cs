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
            _viewModel = new GuestLobbyViewModel(((App)Application.Current).SampleData);
            DataContext = _viewModel;
            Loaded += OnScreenLoaded;
            Closed += OnScreenClosed;
        }

        private void OnScreenLoaded(object sender, RoutedEventArgs e)
        {
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
            Loaded -= OnScreenLoaded;
            Closed -= OnScreenClosed;
        }
    }
}
