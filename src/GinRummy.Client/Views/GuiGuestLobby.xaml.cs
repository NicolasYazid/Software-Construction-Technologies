using System;
using System.Collections.Generic;
using System.Windows;

using GinRummy.Client.Models;
using GinRummy.Client.Services;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Lobby of a guest (P16). Serves CU-02 FA-01 and the guest flows of CU-11 FA-02, CU-17
    /// FA-02 and CU-22 FA-01: the guest reads the chat and looks for a match, but can neither
    /// write nor have friends until it creates an account (CU-01 FA-05).
    /// </summary>
    public partial class GuiGuestLobby : GuiWindowBase
    {
        private readonly IList<ChatEntryDto> _chatEntries;

        /// <summary>
        /// Builds the lobby with the state the guest finds when entering.
        /// </summary>
        public GuiGuestLobby()
        {
            InitializeComponent();
            SampleDataService dataService = new SampleDataService();
            LobbySnapshotDto lobby = dataService.GetGuestLobby();
            _chatEntries = lobby.ChatEntries;
            DataContext = lobby;
            lstMessages.ItemsSource = _chatEntries;
            Loaded += OnScreenLoaded;
            Closed += OnScreenClosed;
        }

        private void OnScreenLoaded(object sender, RoutedEventArgs e)
        {
            if (_chatEntries.Count > 0)
            {
                lstMessages.ScrollIntoView(_chatEntries[_chatEntries.Count - 1]);
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

        private void OnLogInClick(object sender, RoutedEventArgs e)
        {
            GuiLogIn logIn = new GuiLogIn();
            logIn.Owner = Owner;
            logIn.Show();
            Close();
        }

        private void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            GuiSignUp signUp = new GuiSignUp();
            signUp.Owner = Owner;
            signUp.Show();
            Close();
        }

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            // The house rules (P19) arrive with the reference screens.
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            Loaded -= OnScreenLoaded;
            Closed -= OnScreenClosed;
        }
    }
}
