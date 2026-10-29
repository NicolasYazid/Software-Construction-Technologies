using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class GameTableViewModel : ViewModelBase
    {
        private const string CardCountKey = "GameTable_LblCardCount";
        private const string CardCountOneKey = "GameTable_LblCardCountOne";
        private const int SingleCardCount = 1;

        private readonly GameTableSnapshot _table;
        private string _messageDraft;
        private bool _isDecisionPending;
        private bool _isPaused;
        private HandResult _handResult;
        private bool _isHandResultShown;
        private MatchResult _matchResult;

        public GameTableViewModel(SampleDataService dataService)
        {
            if (dataService == null)
            {
                throw new ArgumentNullException(nameof(dataService));
            }

            _table = dataService.GetGameTable();
            _messageDraft = string.Empty;
            _isDecisionPending = true;
            Hand = new ObservableCollection<Card>(_table.Hand);
            DiscardPile = new ObservableCollection<Card> { _table.DiscardTop };
            ChatMessages = new ObservableCollection<ChatMessage>(_table.ChatEntries);
            MatchLog = new ObservableCollection<MatchLogEntry>(_table.MatchLog);
            OpponentCards = Enumerable.Range(0, _table.OpponentCardCount).ToList();
            Hand.CollectionChanged += OnHandChanged;
            TakeCommand = new RelayCommand(Take);
            PassCommand = new RelayCommand(Pass);
            SendMessageCommand = new RelayCommand(SendMessage);
            NextHandCommand = new RelayCommand(StartNextHand);
        }

        public ObservableCollection<Card> Hand { get; private set; }
        public ObservableCollection<Card> DiscardPile { get; private set; }
        public ObservableCollection<ChatMessage> ChatMessages { get; private set; }
        public ObservableCollection<MatchLogEntry> MatchLog { get; private set; }
        public IList<int> OpponentCards { get; private set; }
        public ICommand TakeCommand { get; private set; }
        public ICommand PassCommand { get; private set; }
        public ICommand SendMessageCommand { get; private set; }
        public ICommand NextHandCommand { get; private set; }

        public string OpponentName
        {
            get { return _table.OpponentName; }
        }

        public int OpponentCardCount
        {
            get { return _table.OpponentCardCount; }
        }

        public string OpponentCardCountKey
        {
            get { return ResolveCardCountKey(_table.OpponentCardCount); }
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
            get { return _table.StockCount; }
        }

        public int Deadwood
        {
            get { return _table.Deadwood; }
        }

        public int TargetScore
        {
            get { return _table.TargetScore; }
        }

        public int PlayerScore
        {
            get { return _table.PlayerScore; }
        }

        public int OpponentScore
        {
            get { return _table.OpponentScore; }
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

        public bool IsDecisionPending
        {
            get { return _isDecisionPending; }
            private set
            {
                _isDecisionPending = value;
                RaisePropertyChanged();
            }
        }

        public bool IsPaused
        {
            get { return _isPaused; }
            private set
            {
                _isPaused = value;
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

        // The match resumes when the disconnected opponent returns, or ends if the opponent does not (BD-05).
        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }

        public void ShowHandResult(HandResult handResult)
        {
            HandResult = handResult;
            IsHandResultShown = true;
        }

        public void ShowMatchResult(MatchResult matchResult)
        {
            MatchResult = matchResult;
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

        // The move is not validated here because the server checks it and passes the turn (house rules 2 and 3).
        private void Take()
        {
            Card takenCard = DiscardPile[0];
            DiscardPile.Clear();
            Hand.Add(takenCard);
            IsDecisionPending = false;
        }

        private void Pass()
        {
            MatchLogEntry entry = new MatchLogEntry();
            entry.PlayerName = _table.PlayerName;
            entry.OccurredAt = DateTime.UtcNow;
            entry.Kind = MatchLogKind.Passed;
            MatchLog.Add(entry);
            IsDecisionPending = false;
        }

        // As in the lobby, an empty message is not sent (CU-17 FA-03).
        // The server relays the rest, and until it answers the table shows the message of the player.
        private void SendMessage()
        {
            string content = _messageDraft.Trim();
            if (content.Length > 0)
            {
                ChatMessage message = new ChatMessage();
                message.AuthorName = _table.PlayerName;
                message.Content = content;
                message.SentAt = DateTime.UtcNow;
                ChatMessages.Add(message);
                MessageDraft = string.Empty;
            }
        }

        // The table only carries the score of the hand into the match because the server deals the next hand.
        private void StartNextHand()
        {
            _table.PlayerScore = _handResult.PlayerScore;
            _table.OpponentScore = _handResult.OpponentScore;
            RaisePropertyChanged(nameof(PlayerScore));
            RaisePropertyChanged(nameof(OpponentScore));
            IsHandResultShown = false;
        }
    }
}
