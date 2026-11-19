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

        private static readonly Dictionary<ChallengeOutcome, string> s_outcomeNoticeKeys = new Dictionary<ChallengeOutcome, string>
        {
            { ChallengeOutcome.PlayerNotAvailable, "LobbyChat_PlayerNotAvailable" },
            { ChallengeOutcome.PlayerNotFound, "Error_DataNotFound" },
            { ChallengeOutcome.OpponentBusy, "Error_GameOpponentBusy" },
            { ChallengeOutcome.AccountSuspended, "Error_ModAccountSuspended" },
            { ChallengeOutcome.ChallengeExpired, "Error_GameChallengeExpired" }
        };

        private static readonly Dictionary<DuelFaultCode, string> s_refusalNoticeKeys = new Dictionary<DuelFaultCode, string>
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
            _reconnectionTimer.Tick += HandleReconnectionTimerTick;
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
            _duelConnection.ChallengeReceived += HandleChallengeReceived;
            _duelConnection.ChallengeCancelled += HandleChallengeCancelled;
            _duelConnection.ChallengeDeclined += HandleChallengeDeclined;
            _gameConnection.RoundStarted += HandleRoundStarted;
            _duelConnection.ConnectionLost += HandleDuelConnectionLost;
            _gameConnection.ConnectionLost += HandleGameConnectionLost;
            _duelConnection.Start(_signedInPlayer.AccessToken);
            _gameConnection.Start(_signedInPlayer.AccessToken);
        }

        public void Stop()
        {
            _duelConnection.Stop();
            _gameConnection.Stop();
            _duelConnection.ChallengeReceived -= HandleChallengeReceived;
            _duelConnection.ChallengeCancelled -= HandleChallengeCancelled;
            _duelConnection.ChallengeDeclined -= HandleChallengeDeclined;
            _gameConnection.RoundStarted -= HandleRoundStarted;
            _duelConnection.ConnectionLost -= HandleDuelConnectionLost;
            _gameConnection.ConnectionLost -= HandleGameConnectionLost;
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
                s_outcomeNoticeKeys.TryGetValue(outcome.Value, out noticeKey);
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
            else if (code.HasValue && s_refusalNoticeKeys.ContainsKey(code.Value))
            {
                noticeKey = s_refusalNoticeKeys[code.Value];
            }

            NoticeKey = noticeKey;
        }

        private void HandleChallengeReceived(object sender, Tuple<string, int> challenger)
        {
            RemoveReceived(challenger.Item2);
            LobbyPlayerViewModel player = new LobbyPlayerViewModel();
            player.Username = challenger.Item1;
            player.PublicTag = challenger.Item2;
            ReceivedChallenges.Add(player);
        }

        private void HandleChallengeCancelled(object sender, int challengerPublicTag)
        {
            RemoveReceived(challengerPublicTag);
        }

        private void HandleChallengeDeclined(object sender, int challengedPublicTag)
        {
            if (IsChallengeSent && (_challengedPlayer.PublicTag == challengedPublicTag))
            {
                ChallengedPlayer = null;
                NoticeKey = ChallengeDeclinedKey;
            }
        }

        private void HandleRoundStarted(object sender, RoundDetails startedRound)
        {
            _isPlayingMatch = true;
            ChallengedPlayer = null;
            NoticeKey = null;
            GameTableViewModel table = new GameTableViewModel(_gameConnection, _signedInPlayer);
            table.Start(startedRound);
            RoundStarted?.Invoke(this, table);
        }

        private void HandleDuelConnectionLost(object sender, EventArgs e)
        {
            ChallengedPlayer = null;
            _reconnectionTimer.Start();
        }

        private void HandleGameConnectionLost(object sender, EventArgs e)
        {
            if (!_isPlayingMatch)
            {
                _reconnectionTimer.Start();
            }
        }

        private void HandleReconnectionTimerTick(object sender, EventArgs e)
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
