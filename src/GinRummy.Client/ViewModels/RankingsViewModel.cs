using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows.Input;

using GinRummy.Client.LeaderboardServiceReference;
using GinRummy.Client.Mvvm;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.ViewModels
{
    public class RankingsViewModel : ViewModelBase
    {
        private const int PodiumSize = 3;

        private readonly Func<ILeaderboardService> _leaderboardServiceFactory;
        private readonly ILogger<RankingsViewModel> _logger;
        private List<PlayerRanking> _allRankings;
        private string _searchText;
        private bool _hasNoMatches;

        // A client is created for every load because a client whose channel failed cannot be opened again.
        public RankingsViewModel(Func<ILeaderboardService> leaderboardServiceFactory, ILogger<RankingsViewModel> logger)
        {
            _leaderboardServiceFactory = leaderboardServiceFactory
                ?? throw new ArgumentNullException(nameof(leaderboardServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _allRankings = new List<PlayerRanking>();
            Rankings = new ObservableCollection<PlayerRanking>();
            Podium = new ObservableCollection<PlayerRanking>();
            LoadRankingsCommand = new RelayCommand(LoadRankings);
        }

        public event EventHandler LeaderboardUnavailable;

        public ObservableCollection<PlayerRanking> Rankings { get; private set; }
        public ObservableCollection<PlayerRanking> Podium { get; private set; }
        public ICommand LoadRankingsCommand { get; private set; }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RaisePropertyChanged();
                ShowMatchingRankings();
            }
        }

        public bool HasNoMatches
        {
            get { return _hasNoMatches; }
            private set
            {
                _hasNoMatches = value;
                RaisePropertyChanged();
            }
        }

        // The podium keeps the first three places whatever the search, as the prototype of CU-19 shows it.
        private void LoadRankings()
        {
            _allRankings = LoadRankingsFromService();
            Podium.Clear();
            foreach (PlayerRanking podiumEntry in _allRankings.Take(PodiumSize))
            {
                Podium.Add(podiumEntry);
            }

            ShowMatchingRankings();
        }

        private List<PlayerRanking> LoadRankingsFromService()
        {
            List<PlayerRanking> rankings = new List<PlayerRanking>();
            ILeaderboardService leaderboardService = _leaderboardServiceFactory();
            ICommunicationObject communicationObject = (ICommunicationObject)leaderboardService;
            try
            {
                rankings = leaderboardService.GetLeaderboard();
                communicationObject.Close();
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not read the global leaderboard.");
                NotifyLeaderboardUnavailable();
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The leaderboard service could not be reached to load the global leaderboard.");
                NotifyLeaderboardUnavailable();
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The leaderboard service timed out while loading the global leaderboard.");
                NotifyLeaderboardUnavailable();
            }

            return rankings;
        }

        private void ShowMatchingRankings()
        {
            Rankings.Clear();
            foreach (PlayerRanking ranking in _allRankings.Where(IsSearchMatch))
            {
                Rankings.Add(ranking);
            }

            HasNoMatches = (_allRankings.Count > 0) && (Rankings.Count == 0);
        }

        private bool IsSearchMatch(PlayerRanking ranking)
        {
            bool isMatch = string.IsNullOrWhiteSpace(_searchText)
                || ((ranking.Username != null)
                    && (ranking.Username.IndexOf(_searchText.Trim(), StringComparison.OrdinalIgnoreCase) >= 0));
            return isMatch;
        }

        private void NotifyLeaderboardUnavailable()
        {
            LeaderboardUnavailable?.Invoke(this, EventArgs.Empty);
        }
    }
}
