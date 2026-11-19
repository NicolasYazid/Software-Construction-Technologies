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
        private readonly PlayerProfile _profile;
        private readonly PlayerProfileViewModel _viewModel;

        public GuiPlayerProfile(PlayerProfile profile)
        {
            InitializeComponent();
            _profile = profile;
            _viewModel = ((App)Application.Current).CreatePlayerProfileViewModel(profile);
            _viewModel.ProfileUnavailable += HandleProfileUnavailable;
            DataContext = _viewModel;
            Loaded += HandleProfileLoaded;
        }

        // The profile is loaded once the modal is on screen, because a failed load opens the error dialog over it.
        private void HandleProfileLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= HandleProfileLoaded;
            _viewModel.LoadProfile(((App)Application.Current).SignedInPlayer.AccessToken);
        }

        private void HandleProfileUnavailable(object sender, ErrorDialogKind kind)
        {
            GuiErrorDialog errorDialog = new GuiErrorDialog(kind);
            if (kind == ErrorDialogKind.SessionExpired)
            {
                errorDialog.Closed += HandleSessionExpiredDialogClosed;
            }
            else
            {
                errorDialog.Closed += HandleErrorDialogClosed;
            }

            ShowModal(errorDialog);
        }

        // A profile that could not be read has nothing to show, so the player goes back to the lobby (CU-27 EX-01, EX-02, EX-03).
        private void HandleErrorDialogClosed(object sender, EventArgs e)
        {
            Close();
        }

        // An expired session cannot stay in the lobby, so the lobby closes too (CU-27 EX-04).
        private void HandleSessionExpiredDialogClosed(object sender, EventArgs e)
        {
            LeaveLobby();
        }

        private void HandleEditProfileClick(object sender, RoutedEventArgs e)
        {
            GuiEditProfile editProfile = new GuiEditProfile(_profile);
            editProfile.Closed += HandleEditProfileClosed;
            ShowModal(editProfile);
        }

        private void HandleEditProfileClosed(object sender, EventArgs e)
        {
            _viewModel.RefreshProfile();
        }

        private void HandleSocialLinkClick(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenSocialLink((SocialLink)((FrameworkElement)sender).DataContext);
        }

        private void HandleCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
