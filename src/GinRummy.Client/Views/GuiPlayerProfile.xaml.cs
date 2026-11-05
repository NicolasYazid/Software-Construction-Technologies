using System;
using System.Windows;

using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Profile screen of a player (P21). Implements CU-27 for the profile of another player,
    /// for the own one (FA-02) and for the reduced one of a guest (FA-04), and starts CU-12
    /// and CU-16 from its actions.
    /// </summary>
    public partial class GuiPlayerProfile : GuiModalBase
    {
        private readonly PlayerProfileViewModel _viewModel;

        public GuiPlayerProfile(PlayerProfile profile)
        {
            InitializeComponent();
            _viewModel = ((App)Application.Current).CreatePlayerProfileViewModel(profile);
            _viewModel.ProfileUnavailable += OnProfileUnavailable;
            DataContext = _viewModel;
            Loaded += OnProfileLoaded;
        }

        // The profile is loaded once the modal is on screen, because a failed load opens the error dialog over it.
        private void OnProfileLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnProfileLoaded;
            _viewModel.LoadProfile(((App)Application.Current).SignedInPlayer.AccessToken);
        }

        private void OnProfileUnavailable(object sender, ErrorDialogKind kind)
        {
            GuiErrorDialog errorDialog = new GuiErrorDialog(kind);
            if (kind == ErrorDialogKind.SessionExpired)
            {
                errorDialog.Closed += OnSessionExpiredDialogClosed;
            }
            else
            {
                errorDialog.Closed += OnErrorDialogClosed;
            }

            ShowModal(errorDialog);
        }

        // A profile that could not be read has nothing to show, so the player goes back to the lobby (CU-27 EX-01, EX-02, EX-04).
        private void OnErrorDialogClosed(object sender, EventArgs e)
        {
            Close();
        }

        // An expired session cannot stay in the lobby, so the lobby closes too (CU-27 EX-05).
        private void OnSessionExpiredDialogClosed(object sender, EventArgs e)
        {
            GuiWindowBase lobby = Host;
            Close();
            if (lobby != null)
            {
                lobby.Close();
            }
        }

        private void OnRemoveFriendClick(object sender, RoutedEventArgs e)
        {
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.RemoveFriend);
            confirmDialog.Closed += OnRemoveFriendConfirmClosed;
            ShowModal(confirmDialog);
        }

        private void OnRemoveFriendConfirmClosed(object sender, EventArgs e)
        {
            if (((GuiConfirmDialog)sender).IsConfirmed)
            {
                _viewModel.RemoveFriendCommand.Execute(null);
            }
        }

        private void OnEditProfileClick(object sender, RoutedEventArgs e)
        {
            GuiEditProfile editProfile = new GuiEditProfile();
            ShowModal(editProfile);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
