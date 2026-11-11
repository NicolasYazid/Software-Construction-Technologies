using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

using GinRummy.Client.GameServiceReference;
using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

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
        private const int SingleCardCount = 1;

        private readonly GameConnection _gameConnection;
        private readonly LogInOutcome _signedInPlayer;
        private RoundDetails _round;
        private Card _selectedCard;
        private string _messageDraft;
        private bool _isMoveRefused;
        private int _playerScore;
        private int _opponentScore;
        private HandResult _handResult;
        private string _handHeadlineKey;
        private bool _isHandResultShown;
        private MatchResult _matchResult;

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
            Hand.CollectionChanged += OnHandChanged;
            TakeCommand = new RelayCommand(() => Play(_gameConnection.TakeFirstCard));
            PassCommand = new RelayCommand(() => Play(_gameConnection.PassFirstCard));
            DrawFromStockCommand = new RelayCommand(() => Play(_gameConnection.DrawFromStock));
            DrawFromDiscardPileCommand = new RelayCommand(() => Play(_gameConnection.DrawFromDiscardPile));
            DiscardCommand = new RelayCommand(() => PlaySelectedCard(_gameConnection.Discard), IsCardSelected);
            KnockCommand = new RelayCommand(() => PlaySelectedCard(_gameConnection.Knock), IsCardSelected);
            SendMessageCommand = new RelayCommand(SendMessage);
        }

        public event EventHandler GameUnavailable;

        public ObservableCollection<Card> Hand { get; private set; }
        public ObservableCollection<Card> DiscardPile { get; private set; }
        public ObservableCollection<ChatMessage> ChatMessages { get; private set; }
        public ObservableCollection<MatchLogEntry> MatchLog { get; private set; }
        public IList<int> OpponentCards { get; private set; }
        public ICommand TakeCommand { get; private set; }
        public ICommand PassCommand { get; private set; }
        public ICommand DrawFromStockCommand { get; private set; }
        public ICommand DrawFromDiscardPileCommand { get; private set; }
        public ICommand DiscardCommand { get; private set; }
        public ICommand KnockCommand { get; private set; }
        public ICommand SendMessageCommand { get; private set; }

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
            get { return _round.IsYourTurn && (_round.Step == RoundStep.OfferingFirstCard); }
        }

        public bool IsDrawing
        {
            get { return _round.IsYourTurn && (_round.Step == RoundStep.Drawing); }
        }

        public bool IsDiscarding
        {
            get { return _round.IsYourTurn && (_round.Step == RoundStep.Discarding); }
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
                RaisePropertyChanged();
            }
        }

        public string MessageDraft
        {
            get { return _messageDraft; }
            set
            {
                _messageDraft = value ?? string.Empty;
                RaisePropertyChanged();
            }
        }

        public HandResult HandResult
        {
            get { return _handResult; }
            private set
            {
                _handResult = value;
                RaisePropertyChanged();
            }
        }

        public string HandHeadlineKey
        {
            get { return _handHeadlineKey; }
            private set
            {
                _handHeadlineKey = value;
                RaisePropertyChanged();
            }
        }

        public bool IsHandResultShown
        {
            get { return _isHandResultShown; }
            private set
            {
                _isHandResultShown = value;
                RaisePropertyChanged();
            }
        }

        public MatchResult MatchResult
        {
            get { return _matchResult; }
            private set
            {
                _matchResult = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsMatchResultShown));
                RaisePropertyChanged(nameof(IsVictory));
                RaisePropertyChanged(nameof(IsDefeat));
                RaisePropertyChanged(nameof(IsVictoryByTarget));
                RaisePropertyChanged(nameof(IsVictoryByForfeit));
            }
        }

        public bool IsMatchResultShown
        {
            get { return _matchResult != null; }
        }

        public bool IsDefeat
        {
            get { return IsMatchResultShown && (_matchResult.EndReason == MatchEndReason.OpponentReachedTarget); }
        }

        public bool IsVictory
        {
            get { return IsMatchResultShown && !IsDefeat; }
        }

        public bool IsVictoryByTarget
        {
            get { return IsMatchResultShown && (_matchResult.EndReason == MatchEndReason.PlayerReachedTarget); }
        }

        public bool IsVictoryByForfeit
        {
            get { return IsMatchResultShown && (_matchResult.EndReason == MatchEndReason.OpponentForfeited); }
        }

        // The server answers the subscription with the current state of the round, so the table needs no other first read.
        public void Start()
        {
            _gameConnection.RoundChanged += OnRoundChanged;
            _gameConnection.RoundFinished += OnRoundFinished;
            _gameConnection.OpponentLeft += OnOpponentLeft;
            if (!_gameConnection.Start(_signedInPlayer.AccessToken))
            {
                GameUnavailable?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Stop()
        {
            _gameConnection.Stop();
            _gameConnection.RoundChanged -= OnRoundChanged;
            _gameConnection.RoundFinished -= OnRoundFinished;
            _gameConnection.OpponentLeft -= OnOpponentLeft;
        }

        public void Forfeit()
        {
            _gameConnection.Forfeit(_signedInPlayer.AccessToken);
        }

        public void MoveCard(Card draggedCard, Card targetCard)
        {
            Hand.Move(Hand.IndexOf(draggedCard), Hand.IndexOf(targetCard));
        }

        private void OnHandChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(HandCount));
            RaisePropertyChanged(nameof(HandCountKey));
        }

        // Spanish and English only change the word for a single card.
        // The dictionary keeps that form as a key of its own so that a language with more forms can add them.
        private static string ResolveCardCountKey(int count)
        {
            string key = CardCountKey;
            if (count == SingleCardCount)
            {
                key = CardCountOneKey;
            }

            return key;
        }

        // The move is not validated here because the server checks it and answers with the new state of the round.
        private void Play(Func<string, bool> move)
        {
            _isMoveRefused = !move(_signedInPlayer.AccessToken);
            RaisePropertyChanged(nameof(TurnNoticeKey));
        }

        private void PlaySelectedCard(Func<string, PlayingCard, bool> move)
        {
            _isMoveRefused = !move(_signedInPlayer.AccessToken, ToPlayingCard(_selectedCard));
            RaisePropertyChanged(nameof(TurnNoticeKey));
        }

        private bool IsCardSelected()
        {
            return _selectedCard != null;
        }

        // Until the server relays the chat of the match, the table shows the message of the player.
        private void SendMessage()
        {
            string content = _messageDraft.Trim();
            if (content.Length > 0)
            {
                ChatMessages.Add(new ChatMessage(_signedInPlayer.Username, content, DateTime.UtcNow));
                MessageDraft = string.Empty;
            }
        }

        private void OnRoundChanged(object sender, RoundDetails round)
        {
            _round = round;
            _isMoveRefused = false;
            KeepHandOrder(round.Hand);
            DiscardPile.Clear();
            if (round.TopOfDiscardPile != null)
            {
                DiscardPile.Add(ToCard(round.TopOfDiscardPile));
            }

            OpponentCards = Enumerable.Range(0, round.OpponentCardCount).ToList();
            AddLastMoveToLog(round);

            // An empty name tells WPF that every property of the table changed.
            RaisePropertyChanged(string.Empty);
        }

        private void OnRoundFinished(object sender, RoundSummary summary)
        {
            _round.IsYourTurn = false;
            if (summary.IsWinner)
            {
                _playerScore += summary.Points;
            }
            else
            {
                _opponentScore += summary.Points;
            }

            if (summary.Ending != RoundEnding.Void)
            {
                MatchLogKind closeKind = (summary.Ending == RoundEnding.Gin) ? MatchLogKind.Gin : MatchLogKind.Knocked;
                AddLogEntry(summary.KnockerName, closeKind, null).Amount = summary.KnockerDeadwood;
            }

            HandHeadlineKey = (summary.Ending == RoundEnding.Void) ? RoundVoidKey : HandWinnerKey;
            HandResult = ToHandResult(summary);
            IsHandResultShown = true;
            RaisePropertyChanged(string.Empty);
        }

        private void OnOpponentLeft(object sender, EventArgs e)
        {
            _round.IsYourTurn = false;
            IsHandResultShown = false;
            MatchResult = new MatchResult(MatchEndReason.OpponentForfeited, _playerScore, _opponentScore);
            RaisePropertyChanged(string.Empty);
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

            return result;
        }

        // The cards the player already holds keep the order the player gave them, and a new card goes to the end of the hand.
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
                Hand.Add(card);
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
                List<Card> cards = meld.Select(ToCard).ToList();
                MeldKind kind = cards.All(card => card.Rank == cards[0].Rank) ? MeldKind.Set : MeldKind.Run;
                handMelds.Add(new Meld(kind, cards));
            }

            if (unmatchedCards.Length > 0)
            {
                handMelds.Add(new Meld(MeldKind.Unmatched, unmatchedCards.Select(ToCard).ToList()));
            }

            return handMelds;
        }

        private static bool IsSameCard(Card first, Card second)
        {
            return (first.Rank == second.Rank) && (first.Suit == second.Suit);
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
