using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

using GinRummy.Client.GameServiceReference;
using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

using GameFaultCode = GinRummy.Client.GameServiceReference.ServiceFaultCode;
using ModelCardSuit = GinRummy.Client.Models.CardSuit;
using ServiceCardSuit = GinRummy.Client.GameServiceReference.CardSuit;

namespace GinRummy.Client.ViewModels
{
    public class GameTableViewModel : ViewModelBase
    {
        private const string CardCountKey = "GameTable_LblCardCount";
        private const string CardCountOneKey = "GameTable_LblCardCountOne";
        private const string OpponentTurnKey = "GameTable_LblOpponentTurn";
        private const string InvalidMoveKey = "GameTable_LblInvalidMove";
        private const string HandWinnerKey = "GameTable_LblHandWinner";
        private const string RoundVoidKey = "GameTable_LblRoundVoid";
        private const string KnockTitleKey = "GameTable_LblKnockWith";
        private const string GinTitleKey = "GameTable_LblGinTitle";
        private const string UndercutTitleKey = "GameTable_LblUndercutTitle";
        private const string VoidTitleKey = "GameTable_LblVoidTitle";
        private const string KnockAnnounceKey = "GameTable_LblKnockAnnounce";
        private const string GinAnnounceKey = "GameTable_LblGinAnnounce";
        private const string UndercutAnnounceKey = "GameTable_LblUndercutAnnounce";
        private const string VoidAnnounceKey = "GameTable_LblVoidAnnounce";
        private const string GinBonusKey = "GameTable_LblGinBonus";
        private const string UndercutBonusKey = "GameTable_LblUndercutBonus";
        private const int SingleCardCount = 1;
        private const int PauseNoticeDelaySeconds = 2;

        private readonly GameConnection _gameConnection;
        private readonly LogInOutcome _signedInPlayer;
        private readonly DispatcherTimer _pauseNoticeTimer;
        private RoundDetails _round;
        private Card _selectedCard;
        private string _messageDraft;
        private bool _isMoveRefused;
        private bool _isMatchOver;
        private bool _isOpponentDisconnected;
        private int _playerScore;
        private int _opponentScore;
        private HandResult _handResult;
        private string _handHeadlineKey;
        private bool _isHandResultShown;
        private MatchResult _matchResult;
        private MatchResult _finalMatchResult;

        public GameTableViewModel(GameConnection gameConnection, LogInOutcome signedInPlayer)
        {
            _gameConnection = gameConnection ?? throw new ArgumentNullException(nameof(gameConnection));
            _signedInPlayer = signedInPlayer ?? throw new ArgumentNullException(nameof(signedInPlayer));
            _round = new RoundDetails();
            _messageDraft = string.Empty;
            Hand = new ObservableCollection<Card>();
            DiscardPile = new ObservableCollection<Card>();
            ChatMessages = new ObservableCollection<ChatMessage>();
            MatchLog = new ObservableCollection<MatchLogEntry>();
            OpponentCards = new List<int>();
            HandMelds = new List<Meld>();
            Hand.CollectionChanged += HandleHandChanged;
            _pauseNoticeTimer = new DispatcherTimer();
            _pauseNoticeTimer.Interval = TimeSpan.FromSeconds(PauseNoticeDelaySeconds);
            _pauseNoticeTimer.Tick += HandlePauseNoticeTimerTick;
            TakeCommand = new RelayCommand(() => Play(_gameConnection.TakeFirstCard));
            PassCommand = new RelayCommand(() => Play(_gameConnection.PassFirstCard));
            DrawFromStockCommand = new RelayCommand(() => Play(_gameConnection.DrawFromStock), () => IsDrawing);
            DrawFromDiscardPileCommand = new RelayCommand(() => Play(_gameConnection.DrawFromDiscardPile), () => CanDrawFromDiscardPile);
            DiscardCommand = new RelayCommand(() => PlaySelectedCard(_gameConnection.Discard), CanDiscardSelectedCard);
            KnockCommand = new RelayCommand(() => PlaySelectedCard(_gameConnection.Knock), CanKnockWithSelectedCard);
            SortHandCommand = new RelayCommand(SortHand);
            SendMessageCommand = new RelayCommand(SendMessage);
            CloseHandResultCommand = new RelayCommand(CloseHandResult);
        }

        public event EventHandler<Card> CardDrawnFromStock;
        public event EventHandler<Card> CardTakenFromDiscardPile;
        public event EventHandler ConnectionLost;
        public event EventHandler SessionExpired;
        public event EventHandler MatchAlreadyOver;

        public ObservableCollection<Card> Hand { get; private set; }
        public ObservableCollection<Card> DiscardPile { get; private set; }
        public ObservableCollection<ChatMessage> ChatMessages { get; private set; }
        public ObservableCollection<MatchLogEntry> MatchLog { get; private set; }
        public IList<int> OpponentCards { get; private set; }
        public IList<Meld> HandMelds { get; private set; }
        public ICommand TakeCommand { get; private set; }
        public ICommand PassCommand { get; private set; }
        public ICommand DrawFromStockCommand { get; private set; }
        public ICommand DrawFromDiscardPileCommand { get; private set; }
        public ICommand DiscardCommand { get; private set; }
        public ICommand KnockCommand { get; private set; }
        public ICommand SortHandCommand { get; private set; }
        public ICommand SendMessageCommand { get; private set; }
        public ICommand CloseHandResultCommand { get; private set; }

        public string OpponentName
        {
            get { return _round.OpponentName; }
        }

        public int OpponentCardCount
        {
            get { return _round.OpponentCardCount; }
        }

        public string OpponentCardCountKey
        {
            get { return ResolveCardCountKey(_round.OpponentCardCount); }
        }

        public int HandCount
        {
            get { return Hand.Count; }
        }

        public string HandCountKey
        {
            get { return ResolveCardCountKey(Hand.Count); }
        }

        public int StockCount
        {
            get { return _round.StockCount; }
        }

        public int Deadwood
        {
            get { return _round.Deadwood; }
        }

        public int PlayerScore
        {
            get { return _playerScore; }
        }

        public int OpponentScore
        {
            get { return _opponentScore; }
        }

        public bool IsOfferingFirstCard
        {
            get { return _round.IsYourTurn && (_round.Step == RoundStep.OfferingFirstCard) ? true : false; }
        }

        public bool IsDrawing
        {
            get { return _round.IsYourTurn && (_round.Step == RoundStep.Drawing) ? true : false; }
        }

        public bool IsDiscarding
        {
            get { return _round.IsYourTurn && (_round.Step == RoundStep.Discarding) ? true : false; }
        }

        public bool CanDrawFromDiscardPile
        {
            get { return IsDrawing && (_round.LastMove != RoundMoveKind.PassedFirstCard) ? true : false; }
        }

        public bool CanTakeFromDiscardPile
        {
            get { return IsOfferingFirstCard || CanDrawFromDiscardPile ? true : false; }
        }

        public string TurnNoticeKey
        {
            get
            {
                string noticeKey = null;
                if (!_round.IsYourTurn)
                {
                    noticeKey = OpponentTurnKey;
                }
                else if (_isMoveRefused)
                {
                    noticeKey = InvalidMoveKey;
                }

                return noticeKey;
            }
        }

        public Card SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                _selectedCard = value;
                OnPropertyChanged();
            }
        }

        public string MessageDraft
        {
            get { return _messageDraft; }
            set
            {
                _messageDraft = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public HandResult HandResult
        {
            get { return _handResult; }
            private set
            {
                _handResult = value;
                OnPropertyChanged();
            }
        }

        public string HandHeadlineKey
        {
            get { return _handHeadlineKey; }
            private set
            {
                _handHeadlineKey = value;
                OnPropertyChanged();
            }
        }

        public bool IsHandResultShown
        {
            get { return _isHandResultShown; }
            private set
            {
                _isHandResultShown = value;
                OnPropertyChanged();
            }
        }

        public bool IsMatchOver
        {
            get { return _isMatchOver; }
        }

        public bool IsOpponentDisconnected
        {
            get { return _isOpponentDisconnected; }
            private set
            {
                _isOpponentDisconnected = value;
                OnPropertyChanged();
            }
        }

        public MatchResult MatchResult
        {
            get { return _matchResult; }
            private set
            {
                _matchResult = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsMatchResultShown));
                OnPropertyChanged(nameof(IsVictory));
                OnPropertyChanged(nameof(IsDefeat));
                OnPropertyChanged(nameof(IsVictoryByTarget));
                OnPropertyChanged(nameof(IsVictoryByForfeit));
            }
        }

        public bool IsMatchResultShown
        {
            get { return _matchResult != null ? true : false; }
        }

        public bool IsDefeat
        {
            get { return IsMatchResultShown && (_matchResult.EndReason == MatchEndReason.OpponentReachedTarget) ? true : false; }
        }

        public bool IsVictory
        {
            get { return IsMatchResultShown && !IsDefeat ? true : false; }
        }

        public bool IsVictoryByTarget
        {
            get { return IsMatchResultShown && (_matchResult.EndReason == MatchEndReason.PlayerReachedTarget) ? true : false; }
        }

        public bool IsVictoryByForfeit
        {
            get { return IsMatchResultShown && (_matchResult.EndReason == MatchEndReason.OpponentForfeited) ? true : false; }
        }

        public void Start(RoundDetails startedRound)
        {
            _gameConnection.RoundChanged += HandleRoundChanged;
            _gameConnection.RoundFinished += HandleRoundFinished;
            _gameConnection.OpponentLeft += HandleOpponentLeft;
            _gameConnection.OpponentDisconnected += HandleOpponentDisconnected;
            _gameConnection.OpponentReconnected += HandleOpponentReconnected;
            _gameConnection.ConnectionLost += HandleConnectionLost;
            HandleRoundChanged(this, startedRound);
        }

        public void Stop()
        {
            _gameConnection.RoundChanged -= HandleRoundChanged;
            _gameConnection.RoundFinished -= HandleRoundFinished;
            _gameConnection.OpponentLeft -= HandleOpponentLeft;
            _gameConnection.OpponentDisconnected -= HandleOpponentDisconnected;
            _gameConnection.OpponentReconnected -= HandleOpponentReconnected;
            _gameConnection.ConnectionLost -= HandleConnectionLost;
            _pauseNoticeTimer.Stop();
            if (!_isMatchOver)
            {
                _gameConnection.Forfeit(_signedInPlayer.AccessToken);
            }
        }

        public bool Reconnect()
        {
            bool isReconnected = _gameConnection.Rejoin(_signedInPlayer.AccessToken);
            if (!isReconnected && (_gameConnection.LastFaultCode == GameFaultCode.MatchFinished))
            {
                MatchAlreadyOver?.Invoke(this, EventArgs.Empty);
            }

            return isReconnected;
        }

        public void MoveCard(int oldIndex, int newIndex)
        {
            Hand.Move(oldIndex, newIndex);
        }

        public void TakeFromDiscardPile()
        {
            if (IsOfferingFirstCard)
            {
                Play(_gameConnection.TakeFirstCard);
            }
            else if (IsDrawing)
            {
                Play(_gameConnection.DrawFromDiscardPile);
            }
        }

        public void Discard(Card card)
        {
            ShowMoveOutcome(_gameConnection.Discard(_signedInPlayer.AccessToken, ToPlayingCard(card)));
            if (_isMoveRefused)
            {
                int slot = Hand.IndexOf(card);
                Hand.RemoveAt(slot);
                Hand.Insert(slot, card);
            }
        }

        private void HandleHandChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HandCount));
            OnPropertyChanged(nameof(HandCountKey));
        }

        private static string ResolveCardCountKey(int count)
        {
            string key = CardCountKey;
            if (count == SingleCardCount)
            {
                key = CardCountOneKey;
            }

            return key;
        }

        private void Play(Func<string, bool> move)
        {
            ShowMoveOutcome(move(_signedInPlayer.AccessToken));
        }

        private void PlaySelectedCard(Func<string, PlayingCard, bool> move)
        {
            ShowMoveOutcome(move(_signedInPlayer.AccessToken, ToPlayingCard(_selectedCard)));
        }

        private void ShowMoveOutcome(bool isMoveDone)
        {
            _isMoveRefused = !isMoveDone ? true : false;
            OnPropertyChanged(nameof(TurnNoticeKey));
            if (_isMoveRefused && (_gameConnection.LastFaultCode == GameFaultCode.SessionExpired))
            {
                SessionExpired?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool IsCardSelected()
        {
            return _selectedCard != null ? true : false;
        }

        public bool CanDiscard(Card card)
        {
            return IsDiscarding && !IsTakenFromDiscardPile(card) ? true : false;
        }

        private bool CanDiscardSelectedCard()
        {
            return IsCardSelected() && CanDiscard(_selectedCard) ? true : false;
        }

        private bool IsTakenFromDiscardPile(Card card)
        {
            return (_round.LastMove == RoundMoveKind.DrewFromDiscardPile) && IsSameCard(ToCard(_round.LastMoveCard), card) ? true : false;
        }

        private bool CanKnockWithSelectedCard()
        {
            return IsCardSelected() && _round.KnockDiscards.Any(card => IsSameCard(ToCard(card), _selectedCard)) ? true : false;
        }

        private void SortHand()
        {
            HandOrder.Arrange(Hand, HandOrder.SortLooseCards(Hand, HandMelds.Select(meld => meld.Cards)));
        }

        private void SendMessage()
        {
            string content = _messageDraft.Trim();
            if (content.Length > 0)
            {
                ChatMessages.Add(new ChatMessage(_signedInPlayer.Username, content, DateTime.UtcNow));
                MessageDraft = string.Empty;
            }
        }

        private void HandleRoundChanged(object sender, RoundDetails round)
        {
            _round = round;
            _isMoveRefused = false;
            if (round.LastMove == RoundMoveKind.Dealt)
            {
                DiscardPile.Clear();
            }

            KeepHandOrder(round.Hand);
            ArrangeMelds(round.Melds);
            DiscardPile.Clear();
            if (round.TopOfDiscardPile != null)
            {
                DiscardPile.Add(ToCard(round.TopOfDiscardPile));
            }

            OpponentCards = Enumerable.Range(0, round.OpponentCardCount).ToList();
            AddLastMoveToLog(round);

            OnPropertyChanged(string.Empty);
            CommandManager.InvalidateRequerySuggested();
        }

        private void HandleRoundFinished(object sender, RoundSummary summary)
        {
            _round.IsYourTurn = false;
            _playerScore = summary.PlayerScore;
            _opponentScore = summary.OpponentScore;
            if (summary.IsMatchOver)
            {
                _isMatchOver = true;
                MatchEndReason endReason = summary.IsWinner ? MatchEndReason.PlayerReachedTarget : MatchEndReason.OpponentReachedTarget;
                _finalMatchResult = new MatchResult(endReason, _playerScore, _opponentScore);
            }

            if (summary.Ending != RoundOutcome.Void)
            {
                MatchLogKind closeKind = (summary.Ending == RoundOutcome.Gin) ? MatchLogKind.Gin : MatchLogKind.Knocked;
                AddLogEntry(summary.KnockerName, closeKind, null).Amount = summary.KnockerDeadwood;
            }

            HandHeadlineKey = (summary.Ending == RoundOutcome.Void) ? RoundVoidKey : HandWinnerKey;
            HandResult = ToHandResult(summary);
            IsHandResultShown = true;
            OnPropertyChanged(string.Empty);
        }

        private void HandleOpponentLeft(object sender, EventArgs e)
        {
            _isMatchOver = true;
            _pauseNoticeTimer.Stop();
            _isOpponentDisconnected = false;
            _round.IsYourTurn = false;
            IsHandResultShown = false;
            MatchResult = new MatchResult(MatchEndReason.OpponentForfeited, _playerScore, _opponentScore);
            OnPropertyChanged(string.Empty);
        }

        private void HandleOpponentDisconnected(object sender, EventArgs e)
        {
            _pauseNoticeTimer.Start();
        }

        private void HandlePauseNoticeTimerTick(object sender, EventArgs e)
        {
            _pauseNoticeTimer.Stop();
            IsOpponentDisconnected = true;
        }

        private void HandleOpponentReconnected(object sender, EventArgs e)
        {
            _pauseNoticeTimer.Stop();
            IsOpponentDisconnected = false;
        }

        private void HandleConnectionLost(object sender, EventArgs e)
        {
            if (!_isMatchOver)
            {
                ConnectionLost?.Invoke(this, EventArgs.Empty);
            }
        }

        private void CloseHandResult()
        {
            IsHandResultShown = false;
            if (_finalMatchResult != null)
            {
                MatchResult = _finalMatchResult;
            }
        }

        private HandResult ToHandResult(RoundSummary summary)
        {
            HandResult result = new HandResult();
            result.WinnerName = summary.WinnerName;
            result.KnockerName = summary.KnockerName;
            result.DefenderName = (summary.KnockerName == _round.OpponentName) ? _signedInPlayer.Username : _round.OpponentName;
            result.KnockerDeadwood = summary.KnockerDeadwood;
            result.DefenderDeadwood = summary.DefenderDeadwood;
            result.PointsAwarded = summary.Points;
            result.KnockerMelds = ToMelds(summary.KnockerMelds, summary.KnockerUnmatchedCards);
            result.DefenderMelds = ToMelds(summary.DefenderMelds, summary.DefenderUnmatchedCards);
            result.PlayerScore = _playerScore;
            result.OpponentScore = _opponentScore;
            ExplainScore(result, summary.Ending);

            return result;
        }

        private static void ExplainScore(HandResult result, RoundOutcome ending)
        {
            result.HasScoreBreakdown = ending != RoundOutcome.Void ? true : false;
            switch (ending)
            {
                case RoundOutcome.Gin:
                    result.TitleKey = GinTitleKey;
                    result.AnnounceKey = GinAnnounceKey;
                    result.BaseName = result.DefenderName;
                    result.BaseDeadwood = result.DefenderDeadwood;
                    result.BonusKey = GinBonusKey;
                    result.BonusPoints = result.PointsAwarded - result.DefenderDeadwood;
                    break;
                case RoundOutcome.Undercut:
                    result.TitleKey = UndercutTitleKey;
                    result.AnnounceKey = UndercutAnnounceKey;
                    result.BaseName = result.KnockerName;
                    result.BaseDeadwood = result.KnockerDeadwood;
                    result.SubtractedName = result.DefenderName;
                    result.SubtractedDeadwood = result.DefenderDeadwood;
                    result.BonusKey = UndercutBonusKey;
                    result.BonusPoints = result.PointsAwarded - (result.KnockerDeadwood - result.DefenderDeadwood);
                    break;
                case RoundOutcome.Void:
                    result.TitleKey = VoidTitleKey;
                    result.AnnounceKey = VoidAnnounceKey;
                    break;
                default:
                    result.TitleKey = KnockTitleKey;
                    result.AnnounceKey = KnockAnnounceKey;
                    result.BaseName = result.DefenderName;
                    result.BaseDeadwood = result.DefenderDeadwood;
                    result.SubtractedName = result.KnockerName;
                    result.SubtractedDeadwood = result.KnockerDeadwood;
                    break;
            }
        }

        private void KeepHandOrder(PlayingCard[] serverHand)
        {
            List<Card> dealtCards = serverHand.Select(ToCard).ToList();
            List<Card> leftCards = Hand.Where(held => !dealtCards.Any(dealt => IsSameCard(dealt, held))).ToList();
            foreach (Card card in leftCards)
            {
                Hand.Remove(card);
            }

            List<Card> newCards = dealtCards.Where(dealt => !Hand.Any(held => IsSameCard(held, dealt))).ToList();
            foreach (Card card in newCards)
            {
                AddDrawnCard(card);
            }
        }

        private void ArrangeMelds(PlayingCard[][] melds)
        {
            List<Meld> handMelds = melds.Select(meld => ToMeld(meld.Select(ToCard).Select(FindHeldCard).ToList())).ToList();
            List<Card> meldCards = handMelds.SelectMany(meld => meld.Cards).ToList();
            bool haveMeldsChanged = !new HashSet<Card>(HandMelds.SelectMany(meld => meld.Cards)).SetEquals(meldCards) ? true : false;
            if (haveMeldsChanged && (meldCards.Count > 0))
            {
                HandOrder.Arrange(Hand, HandOrder.PutMeldsFirst(Hand, meldCards));
            }

            HandMelds = handMelds;
        }

        private Card FindHeldCard(Card card)
        {
            return Hand.First(held => IsSameCard(held, card));
        }

        private void AddDrawnCard(Card drawnCard)
        {
            Card topOfDiscardPile = DiscardPile.FirstOrDefault(top => IsSameCard(top, drawnCard));
            if (topOfDiscardPile != null)
            {
                CardTakenFromDiscardPile?.Invoke(this, topOfDiscardPile);
                Hand.Add(topOfDiscardPile);
            }
            else
            {
                CardDrawnFromStock?.Invoke(this, drawnCard);
                Hand.Add(drawnCard);
            }
        }

        private void AddLastMoveToLog(RoundDetails round)
        {
            if (round.LastMove == RoundMoveKind.Dealt)
            {
                AddLogEntry(round.LastMoverName, MatchLogKind.Dealt, null).Amount = round.Hand.Length;
                AddLogEntry(round.LastMoverName, MatchLogKind.TurnedUp, round.TopOfDiscardPile);
            }
            else
            {
                AddLogEntry(round.LastMoverName, ToMatchLogKind(round.LastMove), round.LastMoveCard);
            }
        }

        private MatchLogEntry AddLogEntry(string playerName, MatchLogKind kind, PlayingCard card)
        {
            MatchLogEntry entry = new MatchLogEntry();
            entry.PlayerName = playerName;
            entry.OccurredAt = DateTime.Now;
            entry.Kind = kind;
            if (card != null)
            {
                entry.Card = ToCard(card);
            }

            MatchLog.Add(entry);

            return entry;
        }

        private static MatchLogKind ToMatchLogKind(RoundMoveKind move)
        {
            MatchLogKind kind;
            switch (move)
            {
                case RoundMoveKind.PassedFirstCard:
                    kind = MatchLogKind.Passed;
                    break;
                case RoundMoveKind.DrewFromStock:
                    kind = MatchLogKind.DrewFromStock;
                    break;
                case RoundMoveKind.DrewFromDiscardPile:
                    kind = MatchLogKind.TookDiscard;
                    break;
                default:
                    kind = MatchLogKind.Discarded;
                    break;
            }

            return kind;
        }

        private static IList<Meld> ToMelds(PlayingCard[][] melds, PlayingCard[] unmatchedCards)
        {
            List<Meld> handMelds = new List<Meld>();
            foreach (PlayingCard[] meld in melds)
            {
                handMelds.Add(ToMeld(meld.Select(ToCard).ToList()));
            }

            if (unmatchedCards.Length > 0)
            {
                handMelds.Add(new Meld(MeldKind.Unmatched, unmatchedCards.Select(ToCard).ToList()));
            }

            return handMelds;
        }

        private static Meld ToMeld(IList<Card> cards)
        {
            MeldKind kind = cards.All(card => card.Rank == cards[0].Rank) ? MeldKind.Set : MeldKind.Run;

            return new Meld(kind, cards);
        }

        private static bool IsSameCard(Card first, Card second)
        {
            return (first.Rank == second.Rank) && (first.Suit == second.Suit) ? true : false;
        }

        private static Card ToCard(PlayingCard playingCard)
        {
            return new Card((CardRank)playingCard.Rank, (ModelCardSuit)playingCard.Suit);
        }

        private static PlayingCard ToPlayingCard(Card card)
        {
            PlayingCard playingCard = new PlayingCard();
            playingCard.Rank = (int)card.Rank;
            playingCard.Suit = (ServiceCardSuit)card.Suit;

            return playingCard;
        }
    }
}
