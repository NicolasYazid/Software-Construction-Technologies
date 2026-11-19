using System;
using System.Windows;

using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Leaderboard screen (P18). Implements CU-19: the podium of the first three places and the table
    /// of the global leaderboard or of the leaderboard among friends, filtered by the name the player searches.
    /// </summary>
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

        private void OnLeaderboardUnavailable(object sender, ErrorDialogKind errorDialogKind)
        {
            GuiErrorDialog errorDialog = new GuiErrorDialog(errorDialogKind);
            if (errorDialogKind == ErrorDialogKind.SessionExpired)
            {
                errorDialog.Closed += OnSessionExpiredDialogClosed;
            }

            ShowModal(errorDialog);
        }

        private void OnSessionExpiredDialogClosed(object sender, EventArgs e)
        {
            LeaveLobby();
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
