using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GinRummy.Client.GameServiceReference;
using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class LobbyChallengeViewModel : ViewModelBase
    {
        private const string ChallengeDeclinedKey = "LobbyChat_ChallengeDeclined";
        private const string ChallengeUnavailableKey = "LobbyChat_ChallengeUnavailable";

        private readonly DirectDuelConnection _duelConnection;
        private readonly GameConnection _gameConnection;
        private readonly LogInOutcome _signedInPlayer;
        private LobbyPlayerViewModel _challengedPlayer;
        private string _noticeKey;

        public LobbyChallengeViewModel(DirectDuelConnection duelConnection, GameConnection gameConnection, LogInOutcome signedInPlayer)
        {
            _duelConnection = duelConnection ?? throw new ArgumentNullException(nameof(duelConnection));
            _gameConnection = gameConnection ?? throw new ArgumentNullException(nameof(gameConnection));
            _signedInPlayer = signedInPlayer ?? throw new ArgumentNullException(nameof(signedInPlayer));
            ReceivedChallenges = new ObservableCollection<LobbyPlayerViewModel>();
            CancelCommand = new RelayCommand(Cancel);
            AcceptCommand = new RelayCommand(Accept);
            DeclineCommand = new RelayCommand(Decline);
            DismissNoticeCommand = new RelayCommand(() => NoticeKey = null);
        }

        public event EventHandler<GameTableViewModel> RoundStarted;

        public ObservableCollection<LobbyPlayerViewModel> ReceivedChallenges { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand AcceptCommand { get; private set; }
        public ICommand DeclineCommand { get; private set; }
        public ICommand DismissNoticeCommand { get; private set; }

        public LobbyPlayerViewModel ChallengedPlayer
        {
            get { return _challengedPlayer; }
            private set
            {
                _challengedPlayer = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsChallengeSent));
            }
        }

        public bool IsChallengeSent
        {
            get { return _challengedPlayer != null; }
        }

        public string NoticeKey
        {
            get { return _noticeKey; }
            private set
            {
                _noticeKey = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasNotice));
            }
        }

        public bool HasNotice
        {
            get { return _noticeKey != null; }
        }

        public void Start()
        {
            _duelConnection.ChallengeReceived += OnChallengeReceived;
            _duelConnection.ChallengeCancelled += OnChallengeCancelled;
            _duelConnection.ChallengeDeclined += OnChallengeDeclined;
            _gameConnection.RoundStarted += OnRoundStarted;
            _duelConnection.Start(_signedInPlayer.AccessToken);
            _gameConnection.Start(_signedInPlayer.AccessToken);
        }

        public void Stop()
        {
            _duelConnection.Stop();
            _gameConnection.Stop();
            _duelConnection.ChallengeReceived -= OnChallengeReceived;
            _duelConnection.ChallengeCancelled -= OnChallengeCancelled;
            _duelConnection.ChallengeDeclined -= OnChallengeDeclined;
            _gameConnection.RoundStarted -= OnRoundStarted;
        }

        public void Challenge(LobbyPlayerViewModel player)
        {
            bool isSent = player.PublicTag.HasValue
                && _duelConnection.Challenge(_signedInPlayer.AccessToken, player.PublicTag.Value);
            if (isSent)
            {
                ChallengedPlayer = player;
                NoticeKey = null;
            }
            else
            {
                NoticeKey = ChallengeUnavailableKey;
            }
        }

        private void Cancel()
        {
            _duelConnection.Cancel(_signedInPlayer.AccessToken);
            ChallengedPlayer = null;
        }

        private void Accept(object parameter)
        {
            LobbyPlayerViewModel challenger = (LobbyPlayerViewModel)parameter;
            ReceivedChallenges.Remove(challenger);
            if (!_duelConnection.Accept(_signedInPlayer.AccessToken, challenger.PublicTag.Value))
            {
                NoticeKey = ChallengeUnavailableKey;
            }
        }

        private void Decline(object parameter)
        {
            LobbyPlayerViewModel challenger = (LobbyPlayerViewModel)parameter;
            ReceivedChallenges.Remove(challenger);
            _duelConnection.Decline(_signedInPlayer.AccessToken, challenger.PublicTag.Value);
        }

        private void OnChallengeReceived(object sender, Tuple<string, int> challenger)
        {
            RemoveReceived(challenger.Item2);
            LobbyPlayerViewModel player = new LobbyPlayerViewModel();
            player.Username = challenger.Item1;
            player.PublicTag = challenger.Item2;
            ReceivedChallenges.Add(player);
        }

        private void OnChallengeCancelled(object sender, int challengerPublicTag)
        {
            RemoveReceived(challengerPublicTag);
        }

        private void OnChallengeDeclined(object sender, int challengedPublicTag)
        {
            if (IsChallengeSent && (_challengedPlayer.PublicTag == challengedPublicTag))
            {
                ChallengedPlayer = null;
                NoticeKey = ChallengeDeclinedKey;
            }
        }

        private void OnRoundStarted(object sender, RoundDetails startedRound)
        {
            ChallengedPlayer = null;
            NoticeKey = null;
            GameTableViewModel table = new GameTableViewModel(_gameConnection, _signedInPlayer);
            table.Start(startedRound);
            RoundStarted?.Invoke(this, table);
        }

        private void RemoveReceived(int challengerPublicTag)
        {
            LobbyPlayerViewModel challenger = ReceivedChallenges.FirstOrDefault(
                candidate => candidate.PublicTag == challengerPublicTag);
            if (challenger != null)
            {
                ReceivedChallenges.Remove(challenger);
            }
        }
    }
}
