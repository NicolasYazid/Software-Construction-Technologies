using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows.Input;

using GinRummy.Client.Mvvm;
using GinRummy.Client.RankingsServiceReference;

using Microsoft.Extensions.Logging;

namespace GinRummy.Client.ViewModels
{
    public class RankingsViewModel : ViewModelBase
    {
        private const string ServiceUnavailableMessageKey = "Error_SysServiceUnavailable";
        private const int PodiumSize = 3;

        private readonly IRankingService _rankingService;
        private readonly ILogger<RankingsViewModel> _logger;

        public RankingsViewModel(IRankingService rankingService, ILogger<RankingsViewModel> logger)
        {
            _rankingService = rankingService;
            _logger = logger;
            Rankings = new ObservableCollection<PlayerRankingDto>();
            Podium = new ObservableCollection<PlayerRankingDto>();
            LoadRankingsCommand = new RelayCommand(LoadRankings);
        }

        public ObservableCollection<PlayerRankingDto> Rankings { get; private set; }
        public ObservableCollection<PlayerRankingDto> Podium { get; private set; }
        public ICommand LoadRankingsCommand { get; private set; }

        public string ErrorMessageKey { get; private set; }

        private void LoadRankings()
        {
            List<PlayerRankingDto> rankings = LoadRankingsFromService();
            Rankings.Clear();
            Podium.Clear();
            foreach (PlayerRankingDto ranking in rankings)
            {
                Rankings.Add(ranking);
            }

            foreach (PlayerRankingDto podiumEntry in rankings.Take(PodiumSize))
            {
                Podium.Add(podiumEntry);
            }
        }

        private List<PlayerRankingDto> LoadRankingsFromService()
        {
            List<PlayerRankingDto> rankings;
            ErrorMessageKey = null;
            try
            {
                rankings = _rankingService.GetRankings();
            }
            catch (CommunicationException ex)
            {
                _logger.LogError(ex, "The rankings service could not be reached to load the global leaderboard.");
                rankings = new List<PlayerRankingDto>();
                ErrorMessageKey = ServiceUnavailableMessageKey;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "The rankings service timed out while loading the global leaderboard.");
                rankings = new List<PlayerRankingDto>();
                ErrorMessageKey = ServiceUnavailableMessageKey;
            }

            return rankings;
        }
    }
}
