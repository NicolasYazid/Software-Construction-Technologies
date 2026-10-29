using System;
using System.Windows;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    public partial class GuiRankings : GuiModalBase
    {
        public GuiRankings()
        {
            InitializeComponent();
            RankingsViewModel viewModel = ((App)Application.Current).CreateRankingsViewModel();
            viewModel.LeaderboardUnavailable += OnLeaderboardUnavailable;
            DataContext = viewModel;
            Loaded += OnRankingsLoaded;
        }

        // The leaderboard is loaded once the modal is on screen, because a failed load opens the error dialog over it.
        private void OnRankingsLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnRankingsLoaded;
            RankingsViewModel viewModel = (RankingsViewModel)DataContext;
            viewModel.LoadRankingsCommand.Execute(null);
        }

        private void OnLeaderboardUnavailable(object sender, EventArgs e)
        {
            ShowModal(new GuiErrorDialog(ErrorDialogKind.ServiceUnavailable));
        }

        private void OnTabSelectionChanged(object sender, RoutedEventArgs e)
        {
        }

        private void OnSearchTextChanged(object sender, RoutedEventArgs e)
        {
        }

        private void OnSearchClick(object sender, RoutedEventArgs e)
        {
            txtSearch.Focus();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
