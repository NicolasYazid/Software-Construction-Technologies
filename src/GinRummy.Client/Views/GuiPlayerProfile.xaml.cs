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
            _viewModel = new PlayerProfileViewModel(profile);
            DataContext = _viewModel;
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
