using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using GinRummy.Client.Controllers;
using GinRummy.Client.Models;
using GinRummy.Client.Services;
using GinRummy.Domain.Entities;

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
            ShowLeaderboard(BuildGlobalLeaderboard());
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
                    // TODO: The friends leaderboard needs the FriendShip data and the signed-in player, both server-dependent.
                    // It stays on sample data until then.
                    leaderboard = _dataService.GetFriendsLeaderboard();
                }
                else
                {
                    leaderboard = BuildGlobalLeaderboard();
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

        private LeaderboardDto BuildGlobalLeaderboard()
        {
            App application = (App)Application.Current;
            RankingsController rankingsController = application.CreateRankingsController();
            IList<PlayerStats> rankedStats = rankingsController.GetGlobalRanking();
            if (rankingsController.ErrorMessageKey != null)
            {
                MessageBox.Show(Localization.GetText(rankingsController.ErrorMessageKey));
            }

            List<RankingEntryDto> entries = new List<RankingEntryDto>();
            int position = 1;
            foreach (PlayerStats stats in rankedStats)
            {
                entries.Add(ToRankingEntry(stats, position, rankingsController));
                position++;
            }

            LeaderboardDto leaderboard = new LeaderboardDto();
            leaderboard.Entries = entries;

            // TODO: The highlighted own row (CU-19 FA-03) needs the signed-in player, which is server-dependent.
            // It stays absent until then.
            leaderboard.OwnEntry = null;

            return leaderboard;
        }

        // This conversion lives in the view because the DTO is a presentation shape that must never enter the domain logic.
        private static RankingEntryDto ToRankingEntry(PlayerStats stats, int position, RankingsController controller)
        {
            RankingEntryDto entry = new RankingEntryDto();
            entry.Position = position;
            entry.Username = stats.Player.Username;
            entry.Wins = stats.Wins;
            entry.Losses = stats.Losses;
            entry.WinRate = stats.MatchesPlayed == 0 ? 0 : (double)stats.Wins / stats.MatchesPlayed;
            entry.RankName = controller.ResolveRankName(stats.Score);
            entry.IsOwnEntry = false;

            return entry;
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
