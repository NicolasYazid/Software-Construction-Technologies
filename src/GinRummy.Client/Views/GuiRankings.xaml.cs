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
            viewModel.LeaderboardUnavailable += HandleLeaderboardUnavailable;
            DataContext = viewModel;
            Loaded += HandleRankingsLoaded;
        }

        // The leaderboard is loaded once the modal is on screen, because a failed load opens the error dialog over it.
        private void HandleRankingsLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= HandleRankingsLoaded;
            RankingsViewModel viewModel = (RankingsViewModel)DataContext;
            viewModel.LoadRankingsCommand.Execute(null);
        }

        private void HandleLeaderboardUnavailable(object sender, ErrorDialogKind errorDialogKind)
        {
            GuiErrorDialog errorDialog = new GuiErrorDialog(errorDialogKind);
            if (errorDialogKind == ErrorDialogKind.SessionExpired)
            {
                errorDialog.Closed += HandleSessionExpiredDialogClosed;
            }

            ShowModal(errorDialog);
        }

        private void HandleSessionExpiredDialogClosed(object sender, EventArgs e)
        {
            LeaveLobby();
        }

        private void HandleSearchClick(object sender, RoutedEventArgs e)
        {
            txtSearch.Focus();
        }

        private void HandleCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
