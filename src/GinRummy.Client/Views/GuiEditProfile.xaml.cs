using System;
using System.Windows;

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

        public GuiEditProfile()
        {
            InitializeComponent();
            _viewModel = new EditProfileViewModel(((App)Application.Current).SampleData);
            _viewModel.SocialLinkRemovalRequested += OnSocialLinkRemovalRequested;
            _viewModel.SocialLinkReplaceRequested += OnSocialLinkReplaceRequested;
            DataContext = _viewModel;
            txtSocialUrl.IsVisibleChanged += OnSocialUrlVisibleChanged;
        }

        private void OnChangePhotoClick(object sender, RoutedEventArgs e)
        {
            // The filter shows only the patterns because a filter description is visible text that the dictionary does not carry.
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ImageFilter;
            if (fileDialog.ShowDialog(Window.GetWindow(this)) == true)
            {
                _viewModel.ChangePictureCommand.Execute(fileDialog.FileName);
            }
        }

        private void OnSocialUrlVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (txtSocialUrl.IsVisible)
            {
                txtSocialUrl.Focus();
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
            _viewModel.CompleteSocialLinkRemoval(((GuiConfirmDialog)sender).IsConfirmed);
        }

        private void OnSocialLinkReplaceRequested(object sender, EventArgs e)
        {
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.SocialLinkReplace);
            confirmDialog.Closed += OnReplaceLinkConfirmClosed;
            ShowModal(confirmDialog);
        }

        private void OnReplaceLinkConfirmClosed(object sender, EventArgs e)
        {
            _viewModel.CompleteSocialLinkReplace(((GuiConfirmDialog)sender).IsConfirmed);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
