using System;
using System.Windows;
using System.Windows.Controls;

using GinRummy.Client.Services;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Sanctions screen (P13). Implements CU-33: lists the sanctions of the player, marks the
    /// ones in force and counts down how long they have left. Its first tab is the
    /// notifications screen (P12).
    /// </summary>
    public partial class GuiSanctions : GuiModalBase
    {
        private readonly FriendshipConnection _friendshipConnection;
        private readonly SanctionsViewModel _viewModel;

        public GuiSanctions(FriendshipConnection friendshipConnection)
        {
            InitializeComponent();
            _friendshipConnection = friendshipConnection ?? throw new ArgumentNullException(nameof(friendshipConnection));
            _viewModel = ((App)Application.Current).CreateSanctionsViewModel();
            DataContext = _viewModel;
            Closed += HandleScreenClosed;
        }

        protected override void RefreshFormattedText()
        {
            _viewModel.LoadSanctions();
        }

        private void HandleTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && tabFriendRequests.IsSelected)
            {
                NavigateTo(new GuiNotifications(_friendshipConnection));
            }
        }

        private void HandleCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HandleScreenClosed(object sender, EventArgs e)
        {
            _viewModel.StopCountdown();
            Closed -= HandleScreenClosed;
        }
    }
}
