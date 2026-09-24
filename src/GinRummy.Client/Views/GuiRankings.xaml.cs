using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using GinRummy.Client.Models;
using GinRummy.Client.Services;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Leaderboard screen (P18). Implements CU-19: the leaderboard of every player, the one
    /// among friends (FA-01), the search of a player (FA-02) and the row of the player kept at
    /// the foot of the table when its place lies beyond it (FA-03).
    /// </summary>
    public partial class GuiRankings : GuiWindowBase
    {
        private const int PodiumSize = 3;

        private readonly SampleDataService _dataService;
        private LeaderboardDto _leaderboard;

        /// <summary>
        /// Builds the screen with the leaderboard of every player, the tab it opens on.
        /// </summary>
        public GuiRankings()
        {
            InitializeComponent();
            _dataService = new SampleDataService();
            ShowLeaderboard(_dataService.GetGlobalLeaderboard());
        }

        private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The selection the tab control makes while it loads is not a choice of the player:
            // the constructor already shows the leaderboard of that tab.
            if (IsLoaded)
            {
                LeaderboardDto leaderboard = tabFriends.IsSelected
                    ? _dataService.GetFriendsLeaderboard()
                    : _dataService.GetGlobalLeaderboard();
                ShowLeaderboard(leaderboard);
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearch();
        }

        private void OnSearchClick(object sender, RoutedEventArgs e)
        {
            // The table already filters while the player types, so the icon only takes them to
            // the field.
            txtSearch.Focus();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
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

            // The row of the player is pinned only when the table does not already hold it, so
            // that it never shows twice (CU-19 FA-03).
            bool isOwnEntryPinned = ownEntry != null
                && IsMatch(ownEntry, searchText)
                && !matches.Contains(ownEntry);
            List<RankingEntryDto> pinnedEntries = new List<RankingEntryDto>();
            if (isOwnEntryPinned)
            {
                pinnedEntries.Add(ownEntry);
            }

            lstRankings.ItemsSource = matches;
            lstOwnEntry.ItemsSource = pinnedEntries;
            lblNoMatches.Visibility = VisibilityCommon.FromCondition(matches.Count == 0 && !isOwnEntryPinned);
        }

        private static bool IsMatch(RankingEntryDto entry, string searchText)
        {
            return searchText.Length == 0
                || entry.Username.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
    }
}
