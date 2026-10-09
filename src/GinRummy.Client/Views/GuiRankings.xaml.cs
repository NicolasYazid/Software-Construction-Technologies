using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using GinRummy.Client.Controllers;
using GinRummy.Client.Models;
using GinRummy.Client.Services;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Leaderboard screen (P18). Implements CU-19. The global tab reads real data through the
    /// rankings controller; the friends tab still uses sample data until its backend exists.
    /// </summary>
    public partial class GuiRankings : GuiModalBase
    {
        private const int PodiumSize = 3;

        private readonly SampleDataService _dataService;
        private LeaderboardDto _leaderboard;

        public GuiRankings()
        {
            InitializeComponent();
            _dataService = new SampleDataService();
            ShowLeaderboard(LoadGlobalLeaderboard());
        }

        private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The tab control also raises a selection while it loads, which is not a choice of the player.
            // The constructor already shows the leaderboard of that tab.
            if (IsLoaded)
            {
                LeaderboardDto leaderboard;
                if (tabFriends.IsSelected)
                {
                    // The friends leaderboard stays on sample data because the friendships and the signed-in player come from the server.
                    leaderboard = _dataService.GetFriendsLeaderboard();
                }
                else
                {
                    leaderboard = LoadGlobalLeaderboard();
                }

                ShowLeaderboard(leaderboard);
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearch();
        }

        private void OnSearchClick(object sender, RoutedEventArgs e)
        {
            // The table already filters while the player types.
            // The icon therefore only takes the player to the field.
            txtSearch.Focus();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private LeaderboardDto LoadGlobalLeaderboard()
        {
            App application = (App)Application.Current;
            RankingsController rankingsController = application.CreateRankingsController();
            LeaderboardDto leaderboard = rankingsController.GetGlobalLeaderboard();
            if (rankingsController.ErrorMessageKey != null)
            {
                MessageBox.Show(Localization.GetText(rankingsController.ErrorMessageKey));
            }

            return leaderboard;
        }

        private void ShowLeaderboard(LeaderboardDto leaderboard)
        {
            _leaderboard = leaderboard;
            lstPodium.ItemsSource = leaderboard.Entries.Take(PodiumSize).ToList();
            ApplySearch();
        }

        private void ApplySearch()
        {
            string searchText = txtSearch.Text.Trim();
            RankingEntryDto ownEntry = _leaderboard.OwnEntry;
            List<RankingEntryDto> matches = _leaderboard.Entries
                .Where(entry => IsMatch(entry, searchText))
                .ToList();

            // The row of the player is pinned only when the table does not already hold it (CU-19 FA-03).
            // That way the row never shows twice.
            bool isOwnEntryPinned = (ownEntry != null)
                && IsMatch(ownEntry, searchText)
                && !matches.Contains(ownEntry);
            List<RankingEntryDto> pinnedEntries = new List<RankingEntryDto>();
            if (isOwnEntryPinned)
            {
                pinnedEntries.Add(ownEntry);
            }

            lstRankings.ItemsSource = matches;
            lstOwnEntry.ItemsSource = pinnedEntries;
            lblNoMatches.Visibility = VisibilityCommon.FromCondition((matches.Count == 0) && !isOwnEntryPinned);
        }

        private static bool IsMatch(RankingEntryDto entry, string searchText)
        {
            return (searchText.Length == 0)
                || (entry.Username.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0);
        }
    }
}
