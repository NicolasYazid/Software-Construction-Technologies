using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows.Input;

using GinRummy.Client.Mvvm;
using GinRummy.Client.LeaderboardServiceReference;

using Microsoft.Extensions.Logging;

namespace GinRummy.Client.ViewModels
{
    public class RankingsViewModel : ViewModelBase
    {
        private const string ServiceUnavailableMessageKey = "Error_SysServiceUnavailable";
        private const int PodiumSize = 3;

        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<RankingsViewModel> _logger;

        public RankingsViewModel(ILeaderboardService leaderboardService, ILogger<RankingsViewModel> logger)
        {
            _leaderboardService = leaderboardService;
            _logger = logger;
            Rankings = new ObservableCollection<PlayerRanking>();
            Podium = new ObservableCollection<PlayerRanking>();
            LoadRankingsCommand = new RelayCommand(LoadRankings);
        }

        public ObservableCollection<PlayerRanking> Rankings { get; private set; }
        public ObservableCollection<PlayerRanking> Podium { get; private set; }
        public ICommand LoadRankingsCommand { get; private set; }

        public string ErrorMessageKey { get; private set; }

        private void LoadRankings()
        {
            List<PlayerRanking> rankings = LoadRankingsFromService();
            Rankings.Clear();
            Podium.Clear();
            foreach (PlayerRanking ranking in rankings)
            {
                Rankings.Add(ranking);
            }

            foreach (PlayerRanking podiumEntry in rankings.Take(PodiumSize))
            {
                Podium.Add(podiumEntry);
            }
        }

        private List<PlayerRanking> LoadRankingsFromService()
        {
            List<PlayerRanking> rankings;
            ErrorMessageKey = null;
            try
            {
                rankings = _leaderboardService.GetLeaderboard();
            }
            catch (CommunicationException ex)
            {
                _logger.LogError(ex, "The rankings service could not be reached to load the global leaderboard.");
                rankings = new List<PlayerRanking>();
                ErrorMessageKey = ServiceUnavailableMessageKey;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "The rankings service timed out while loading the global leaderboard.");
                rankings = new List<PlayerRanking>();
                ErrorMessageKey = ServiceUnavailableMessageKey;
            }

            return rankings;
        }
    }
}
