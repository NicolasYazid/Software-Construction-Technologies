using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

using GinRummy.Client.GameServiceReference;
using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

using ChallengeOutcome = GinRummy.Client.DirectDuelServiceReference.ChallengeOutcome;
using DuelFaultCode = GinRummy.Client.DirectDuelServiceReference.ServiceFaultCode;
using GameFaultCode = GinRummy.Client.GameServiceReference.ServiceFaultCode;

namespace GinRummy.Client.ViewModels
{
    public class LobbyChallengeViewModel : ViewModelBase
    {
        private const string ChallengeDeclinedKey = "LobbyChat_ChallengeDeclined";
        private const string ChallengeUnavailableKey = "LobbyChat_ChallengeUnavailable";
        private const string ChallengeCancelledKey = "LobbyChat_ChallengeCancelled";
        private const int SecondsBetweenReconnectionAttempts = 10;

        private static readonly Dictionary<ChallengeOutcome, string> _outcomeNoticeKeys = new Dictionary<ChallengeOutcome, string>
        {
            { ChallengeOutcome.PlayerNotAvailable, "LobbyChat_PlayerNotAvailable" },
            { ChallengeOutcome.PlayerNotFound, "Error_DataNotFound" },
            { ChallengeOutcome.OpponentBusy, "Error_GameOpponentBusy" },
            { ChallengeOutcome.AccountSuspended, "Error_ModAccountSuspended" },
            { ChallengeOutcome.ChallengeExpired, "Error_GameChallengeExpired" }
        };

        private static readonly Dictionary<DuelFaultCode, string> _refusalNoticeKeys = new Dictionary<DuelFaultCode, string>
        {
            { DuelFaultCode.CannotChallengeSelf, "Error_GameCannotChallengeSelf" },
            { DuelFaultCode.ConcurrentChallenge, "Error_DataConcurrentChallenge" },
            { DuelFaultCode.GuestNotAllowed, "Error_PermGuestNotAllowed" }
        };

        private readonly DirectDuelConnection _duelConnection;
        private readonly GameConnection _gameConnection;
        private readonly LogInOutcome _signedInPlayer;
        private readonly DispatcherTimer _reconnectionTimer;
        private LobbyPlayerViewModel _challengedPlayer;
        private string _noticeKey;
        private bool _isPlayingMatch;

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
            _reconnectionTimer = new DispatcherTimer();
            _reconnectionTimer.Interval = TimeSpan.FromSeconds(SecondsBetweenReconnectionAttempts);
            _reconnectionTimer.Tick += OnReconnectionTimerTick;
        }

        public event EventHandler<GameTableViewModel> RoundStarted;
        public event EventHandler SessionExpired;

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
            _duelConnection.ConnectionLost += OnDuelConnectionLost;
            _gameConnection.ConnectionLost += OnGameConnectionLost;
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
            _duelConnection.ConnectionLost -= OnDuelConnectionLost;
            _gameConnection.ConnectionLost -= OnGameConnectionLost;
            _reconnectionTimer.Stop();
        }

        public void LeaveMatch()
        {
            _isPlayingMatch = false;
            if (!_duelConnection.IsConnected || !_gameConnection.IsConnected)
            {
                _reconnectionTimer.Start();
            }
        }

        public void Challenge(LobbyPlayerViewModel player)
        {
            if (!player.PublicTag.HasValue)
            {
                NoticeKey = ChallengeUnavailableKey;
            }
            else
            {
                ChallengeOutcome? outcome = _duelConnection.Challenge(_signedInPlayer.AccessToken, player.PublicTag.Value);
                if (outcome == ChallengeOutcome.Done)
                {
                    ChallengedPlayer = player;
                }

                ShowOutcome(outcome);
            }
        }

        private void Cancel()
        {
            _duelConnection.Cancel(_signedInPlayer.AccessToken);
            ChallengedPlayer = null;
            NoticeKey = ChallengeCancelledKey;
        }

        private void Accept(object parameter)
        {
            LobbyPlayerViewModel challenger = (LobbyPlayerViewModel)parameter;
            ReceivedChallenges.Remove(challenger);
            ShowOutcome(_duelConnection.Accept(_signedInPlayer.AccessToken, challenger.PublicTag.Value));
        }

        private void Decline(object parameter)
        {
            LobbyPlayerViewModel challenger = (LobbyPlayerViewModel)parameter;
            ReceivedChallenges.Remove(challenger);
            ShowOutcome(_duelConnection.Decline(_signedInPlayer.AccessToken, challenger.PublicTag.Value));
        }

        private void ShowOutcome(ChallengeOutcome? outcome)
        {
            if (outcome.HasValue)
            {
                string noticeKey;
                _outcomeNoticeKeys.TryGetValue(outcome.Value, out noticeKey);
                NoticeKey = noticeKey;
            }
            else
            {
                ShowRefusal();
            }
        }

        private void ShowRefusal()
        {
            DuelFaultCode? code = _duelConnection.LastFaultCode;
            string noticeKey = ChallengeUnavailableKey;
            if (code == DuelFaultCode.SessionExpired)
            {
                noticeKey = null;
                SessionExpired?.Invoke(this, EventArgs.Empty);
            }
            else if (code.HasValue && _refusalNoticeKeys.ContainsKey(code.Value))
            {
                noticeKey = _refusalNoticeKeys[code.Value];
            }

            NoticeKey = noticeKey;
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
            _isPlayingMatch = true;
            ChallengedPlayer = null;
            NoticeKey = null;
            GameTableViewModel table = new GameTableViewModel(_gameConnection, _signedInPlayer);
            table.Start(startedRound);
            RoundStarted?.Invoke(this, table);
        }

        private void OnDuelConnectionLost(object sender, EventArgs e)
        {
            ChallengedPlayer = null;
            _reconnectionTimer.Start();
        }

        private void OnGameConnectionLost(object sender, EventArgs e)
        {
            if (!_isPlayingMatch)
            {
                _reconnectionTimer.Start();
            }
        }

        private void OnReconnectionTimerTick(object sender, EventArgs e)
        {
            bool isDuelConnected = _duelConnection.IsConnected || _duelConnection.Start(_signedInPlayer.AccessToken);
            bool isGameConnected = _isPlayingMatch || _gameConnection.IsConnected || _gameConnection.Start(_signedInPlayer.AccessToken);
            bool isSessionExpired = (_duelConnection.LastFaultCode == DuelFaultCode.SessionExpired)
                || (_gameConnection.LastFaultCode == GameFaultCode.SessionExpired);
            if (isSessionExpired || (isDuelConnected && isGameConnected))
            {
                _reconnectionTimer.Stop();
            }

            if (isSessionExpired)
            {
                SessionExpired?.Invoke(this, EventArgs.Empty);
            }
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
