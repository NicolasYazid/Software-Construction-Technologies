using System;
using System.Windows;
using System.Windows.Controls;

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
        private readonly SanctionsViewModel _viewModel;

        public GuiSanctions()
        {
            InitializeComponent();
            _viewModel = new SanctionsViewModel(((App)Application.Current).SampleData);
            DataContext = _viewModel;
            Closed += OnScreenClosed;
        }

        protected override void RefreshFormattedText()
        {
            _viewModel.LoadSanctions();
        }

        private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && tabFriendRequests.IsSelected)
            {
                NavigateTo(new GuiNotifications());
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.StopCountdown();
            Closed -= OnScreenClosed;
        }
    }
}
