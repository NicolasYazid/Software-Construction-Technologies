using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows.Input;

using GinRummy.Client.LeaderboardServiceReference;
using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.ViewModels
{
    public class RankingsViewModel : ViewModelBase
    {
        private const int PodiumSize = 3;
        private const int OwnEntryOnly = 1;

        private readonly Func<ILeaderboardService> _leaderboardServiceFactory;
        private readonly string _accessToken;
        private readonly ILogger<RankingsViewModel> _logger;
        private List<PlayerRanking> _globalRankings;
        private List<PlayerRanking> _friendRankings;
        private List<PlayerRanking> _allRankings;
        private string _searchText;
        private bool _hasNoMatches;
        private bool _isFriendsTabSelected;
        private bool _hasNoFriends;
        private bool _isLeaderboardUnavailable;

        // A client is created for every load because a client whose channel failed cannot be opened again.
        public RankingsViewModel(
            Func<ILeaderboardService> leaderboardServiceFactory,
            string accessToken,
            ILogger<RankingsViewModel> logger)
        {
            _leaderboardServiceFactory = leaderboardServiceFactory
                ?? throw new ArgumentNullException(nameof(leaderboardServiceFactory));
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _globalRankings = new List<PlayerRanking>();
            _friendRankings = new List<PlayerRanking>();
            _allRankings = new List<PlayerRanking>();
            Rankings = new ObservableCollection<PlayerRanking>();
            Podium = new ObservableCollection<PlayerRanking>();
            LoadRankingsCommand = new RelayCommand(LoadRankings);
        }

        public event EventHandler<ErrorDialogKind> LeaderboardUnavailable;

        public ObservableCollection<PlayerRanking> Rankings { get; private set; }
        public ObservableCollection<PlayerRanking> Podium { get; private set; }
        public ICommand LoadRankingsCommand { get; private set; }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ShowMatchingRankings();
            }
        }

        public bool HasNoMatches
        {
            get { return _hasNoMatches; }
            private set
            {
                _hasNoMatches = value;
                OnPropertyChanged();
            }
        }

        public bool IsFriendsTabSelected
        {
            get { return _isFriendsTabSelected; }
            set
            {
                _isFriendsTabSelected = value;
                OnPropertyChanged();
                ShowSelectedLeaderboard();
            }
        }

        public bool HasNoFriends
        {
            get { return _hasNoFriends; }
            private set
            {
                _hasNoFriends = value;
                OnPropertyChanged();
            }
        }

        private void LoadRankings()
        {
            _globalRankings = LoadRankingsFromService(LeaderboardType.Global);
            if (!_isLeaderboardUnavailable)
            {
                _friendRankings = LoadRankingsFromService(LeaderboardType.Friends);
            }

            ShowSelectedLeaderboard();
        }

        private void ShowSelectedLeaderboard()
        {
            _allRankings = _globalRankings;
            if (_isFriendsTabSelected)
            {
                _allRankings = _friendRankings;
            }

            Podium.Clear();
            foreach (PlayerRanking podiumEntry in _allRankings.Take(PodiumSize))
            {
                Podium.Add(podiumEntry);
            }

            ShowMatchingRankings();
        }

        private List<PlayerRanking> LoadRankingsFromService(LeaderboardType leaderboardType)
        {
            List<PlayerRanking> rankings = new List<PlayerRanking>();
            ILeaderboardService leaderboardService = _leaderboardServiceFactory();
            ICommunicationObject communicationObject = (ICommunicationObject)leaderboardService;
            try
            {
                rankings = leaderboardService.GetLeaderboard(_accessToken, leaderboardType);
                communicationObject.Close();
            }
            catch (FaultException<ServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogWarning(
                    ex,
                    "The server refused the {LeaderboardType} leaderboard with the fault {FaultCode}.",
                    leaderboardType,
                    ex.Detail.Code);
                NotifyLeaderboardUnavailable(FindErrorDialogKind(ex.Detail.Code));
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not read the {LeaderboardType} leaderboard.", leaderboardType);
                NotifyLeaderboardUnavailable(ErrorDialogKind.ServiceUnavailable);
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(
                    ex,
                    "The leaderboard service could not be reached to load the {LeaderboardType} leaderboard.",
                    leaderboardType);
                NotifyLeaderboardUnavailable(ErrorDialogKind.ServiceUnavailable);
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(
                    ex,
                    "The leaderboard service timed out while loading the {LeaderboardType} leaderboard.",
                    leaderboardType);
                NotifyLeaderboardUnavailable(ErrorDialogKind.ServiceUnavailable);
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
            HasNoFriends = _isFriendsTabSelected
                && (_friendRankings.Count == OwnEntryOnly)
                && (Rankings.Count == OwnEntryOnly);
        }

        private bool IsSearchMatch(PlayerRanking ranking)
        {
            bool isMatch = string.IsNullOrWhiteSpace(_searchText)
                || ((ranking.Username != null)
                    && (ranking.Username.IndexOf(_searchText.Trim(), StringComparison.OrdinalIgnoreCase) >= 0));
            return isMatch;
        }

        private static ErrorDialogKind FindErrorDialogKind(ServiceFaultCode faultCode)
        {
            ErrorDialogKind errorDialogKind = ErrorDialogKind.ServiceUnavailable;
            if (faultCode == ServiceFaultCode.SessionExpired)
            {
                errorDialogKind = ErrorDialogKind.SessionExpired;
            }

            return errorDialogKind;
        }

        private void NotifyLeaderboardUnavailable(ErrorDialogKind errorDialogKind)
        {
            _isLeaderboardUnavailable = true;
            LeaderboardUnavailable?.Invoke(this, errorDialogKind);
        }
    }
}
