using System;
using System.Windows;

using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;
using Microsoft.Win32;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Edit profile screen (P15). Serves CU-28, CU-29, CU-30, CU-31 and CU-32: the picture,
    /// the name, the biography and the social links of the player.
    /// </summary>
    public partial class GuiEditProfile : GuiModalBase
    {
        private const string ImageFilter = "*.jpg;*.jpeg;*.png|*.jpg;*.jpeg;*.png";

        private readonly EditProfileViewModel _viewModel;

        public GuiEditProfile(PlayerProfile profile)
        {
            InitializeComponent();
            _viewModel = ((App)Application.Current).CreateEditProfileViewModel(profile);
            _viewModel.SocialLinkRemovalRequested += HandleSocialLinkRemovalRequested;
            _viewModel.ErrorDialogRequested += HandleErrorDialogRequested;
            DataContext = _viewModel;
            txtSocialPlatformName.IsVisibleChanged += HandleSocialPlatformNameVisibleChanged;
        }

        private static string AccessToken
        {
            get { return ((App)Application.Current).SignedInPlayer.AccessToken; }
        }

        private void HandleChangePhotoClick(object sender, RoutedEventArgs e)
        {
            // The filter shows only the patterns because a filter description is visible text that the language resources do not carry.
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ImageFilter;
            if (fileDialog.ShowDialog(Window.GetWindow(this)) == true)
            {
                _viewModel.ChangePicture(AccessToken, fileDialog.FileName);
            }
        }

        private void HandleRemovePhotoClick(object sender, RoutedEventArgs e)
        {
            _viewModel.RemovePicture(AccessToken);
        }

        private void HandleSaveClick(object sender, RoutedEventArgs e)
        {
            _viewModel.Save(AccessToken);
        }

        private void HandleSocialPlatformNameVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (txtSocialPlatformName.IsVisible)
            {
                txtSocialPlatformName.Focus();
            }
        }

        private void HandleSocialLinkRemovalRequested(object sender, EventArgs e)
        {
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.SocialLinkRemove);
            confirmDialog.Closed += HandleUnlinkConfirmClosed;
            ShowModal(confirmDialog);
        }

        private void HandleUnlinkConfirmClosed(object sender, EventArgs e)
        {
            _viewModel.CompleteSocialLinkRemoval(((GuiConfirmDialog)sender).IsConfirmed, AccessToken);
        }

        private void HandleErrorDialogRequested(object sender, ErrorDialogKind kind)
        {
            GuiErrorDialog errorDialog = new GuiErrorDialog(kind);
            if (kind == ErrorDialogKind.SessionExpired)
            {
                errorDialog.Closed += HandleSessionExpiredDialogClosed;
            }

            ShowModal(errorDialog);
        }

        // An expired session cannot stay in the lobby, so the lobby closes too.
        private void HandleSessionExpiredDialogClosed(object sender, EventArgs e)
        {
            LeaveLobby();
        }

        private void HandleCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
