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
            _viewModel.SocialLinkRemovalRequested += OnSocialLinkRemovalRequested;
            _viewModel.ErrorDialogRequested += OnErrorDialogRequested;
            DataContext = _viewModel;
            txtSocialPlatformName.IsVisibleChanged += OnSocialPlatformNameVisibleChanged;
        }

        private static string AccessToken
        {
            get { return ((App)Application.Current).SignedInPlayer.AccessToken; }
        }

        private void OnChangePhotoClick(object sender, RoutedEventArgs e)
        {
            // The filter shows only the patterns because a filter description is visible text that the dictionary does not carry.
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ImageFilter;
            if (fileDialog.ShowDialog(Window.GetWindow(this)) == true)
            {
                _viewModel.ChangePicture(AccessToken, fileDialog.FileName);
            }
        }

        private void OnRemovePhotoClick(object sender, RoutedEventArgs e)
        {
            _viewModel.RemovePicture(AccessToken);
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            _viewModel.Save(AccessToken);
        }

        private void OnSocialPlatformNameVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (txtSocialPlatformName.IsVisible)
            {
                txtSocialPlatformName.Focus();
            }
        }

        private void OnSocialLinkRemovalRequested(object sender, EventArgs e)
        {
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.SocialLinkRemove);
            confirmDialog.Closed += OnUnlinkConfirmClosed;
            ShowModal(confirmDialog);
        }

        private void OnUnlinkConfirmClosed(object sender, EventArgs e)
        {
            _viewModel.CompleteSocialLinkRemoval(((GuiConfirmDialog)sender).IsConfirmed, AccessToken);
        }

        private void OnErrorDialogRequested(object sender, ErrorDialogKind kind)
        {
            GuiErrorDialog errorDialog = new GuiErrorDialog(kind);
            if (kind == ErrorDialogKind.SessionExpired)
            {
                errorDialog.Closed += OnSessionExpiredDialogClosed;
            }

            ShowModal(errorDialog);
        }

        // An expired session cannot stay in the lobby, so the lobby closes too.
        private void OnSessionExpiredDialogClosed(object sender, EventArgs e)
        {
            GuiWindowBase lobby = Host;
            Close();
            if (lobby != null)
            {
                lobby.Close();
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
