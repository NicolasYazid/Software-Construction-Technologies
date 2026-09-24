using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Services;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Game table screen (P20). Shows a match from the side of one of its players: the hand,
    /// what can be seen of the opponent, the stock and the discard pile, the score, the log of
    /// the match and its chat (CU-17 FA-01). From it the player reads the rules (CU-21 FA-01)
    /// and forfeits the match (CU-26). Over it open the pause of a match whose opponent lost
    /// the connection, the close of each hand and the result of the match.
    /// </summary>
    public partial class GuiGameTable : GuiWindowBase
    {
        private const string CardCountKey = "GameTable_LblCardCount";
        private const string CardCountOneKey = "GameTable_LblCardCountOne";
        private const string StockKey = "GameTable_LblStock";
        private const string DeadwoodInlineKey = "GameTable_LblDeadwoodInline";
        private const string HandWinnerKey = "GameTable_LblHandWinner";
        private const string KnockWithKey = "GameTable_LblKnockWith";
        private const string KnockAnnounceKey = "GameTable_LblKnockAnnounce";
        private const string MinusOpponentDeadwoodKey = "GameTable_LblMinusOpponentDeadwood";
        private const string PointsForKey = "GameTable_LblPointsFor";
        private const string HandRoleKnockedKey = "GameTable_LblHandRoleKnocked";
        private const string DeadwoodCountKey = "GameTable_LblDeadwoodCount";
        private const string ScoreTargetKey = "GameTable_LblScoreTarget";
        private const string FinalScoreKey = "GameTable_LblFinalScore";
        private const string CountFormat = "N0";
        private const string SubtractedFormat = "-#,0;-#,0;0";
        private const string EarnedFormat = "+#,0;-#,0;0";

        private readonly GameTableSnapshotDto _table;
        private readonly ObservableCollection<CardDto> _hand;
        private readonly ObservableCollection<CardDto> _discardPile;
        private readonly ObservableCollection<ChatMessageDto> _chatEntries;
        private readonly ObservableCollection<MatchLogEntryDto> _matchLog;
        private HandResultDto _handResult;
        private MatchResultDto _matchResult;
        private Point _dragStart;
        private CardDto _pressedCard;

        /// <summary>
        /// Builds the table of the match the player has just entered.
        /// </summary>
        public GuiGameTable()
        {
            InitializeComponent();
            SampleDataService dataService = new SampleDataService();
            _table = dataService.GetGameTable();
            _hand = new ObservableCollection<CardDto>(_table.Hand);
            _discardPile = new ObservableCollection<CardDto>();
            _discardPile.Add(_table.DiscardTop);
            _chatEntries = new ObservableCollection<ChatMessageDto>(_table.ChatEntries);
            _matchLog = new ObservableCollection<MatchLogEntryDto>(_table.MatchLog);
            DataContext = _table;
            lstHand.ItemsSource = _hand;
            lstDiscard.ItemsSource = _discardPile;
            lstMessages.ItemsSource = _chatEntries;
            lstMatchLog.ItemsSource = _matchLog;
            lstOpponentCards.ItemsSource = Enumerable.Range(0, _table.OpponentCardCount).ToList();
            RefreshFormattedText();
            Loaded += OnScreenLoaded;
            Closed += OnScreenClosed;
        }

        /// <summary>
        /// Pauses the table while the opponent is disconnected (BD-05). The match resumes when
        /// the opponent returns, or ends if the opponent does not.
        /// </summary>
        public void ShowPaused()
        {
            lblPaused.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Takes the pause off the table once the opponent is back.
        /// </summary>
        public void HidePaused()
        {
            lblPaused.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the close of a hand: who wins it, how its points are counted, the groups of both
        /// hands and the score of the match afterwards.
        /// </summary>
        /// <param name="handResult">Count of the hand, as the server closes it.</param>
        public void ShowHandResult(HandResultDto handResult)
        {
            _handResult = handResult;
            lstKnockerMelds.ItemsSource = handResult.KnockerMelds;
            lstDefenderMelds.ItemsSource = handResult.DefenderMelds;
            lblKnockWith.Visibility = Visibility.Visible;
            RefreshFormattedText();
        }

        /// <summary>
        /// Announces the end of the match with its verdict, the reason for it and the final
        /// score.
        /// </summary>
        /// <param name="matchResult">Result of the match, as the server ends it.</param>
        public void ShowMatchResult(MatchResultDto matchResult)
        {
            _matchResult = matchResult;
            MatchEndReason endReason = matchResult.EndReason;
            bool isDefeat = endReason == MatchEndReason.OpponentReachedTarget;
            lblResultVictory.Visibility = VisibilityCommon.FromCondition(!isDefeat);
            lblResultDefeat.Visibility = VisibilityCommon.FromCondition(isDefeat);
            lblVictoryReasonTarget.Visibility = VisibilityCommon.FromCondition(endReason == MatchEndReason.PlayerReachedTarget);
            lblVictoryReasonForfeit.Visibility = VisibilityCommon.FromCondition(endReason == MatchEndReason.OpponentForfeited);
            lblDefeatReason.Visibility = VisibilityCommon.FromCondition(isDefeat);
            lblFinalScore.Visibility = Visibility.Visible;
            RefreshFormattedText();
        }

        /// <summary>
        /// Rebuilds the counters of the table, the count of a closed hand and the final score,
        /// whose words, separators, signs and case depend on the culture.
        /// </summary>
        protected override void RefreshFormattedText()
        {
            CultureInfo culture = Localization.CurrentCulture;
            lblCardCount.Text = FormatCardCount(_table.OpponentCardCount);
            lblYourCardCount.Text = FormatCardCount(_hand.Count);
            lblStock.Text = Localization.Format(StockKey, _table.StockCount).ToUpper(culture);
            lblDeadwoodInline.Text = Localization.Format(DeadwoodInlineKey, _table.Deadwood).ToUpper(culture);
            lblPlayerScoreValue.Text = _table.PlayerScore.ToString(CountFormat, culture);
            lblOpponentScoreValue.Text = _table.OpponentScore.ToString(CountFormat, culture);
            lblTargetValue.Text = _table.TargetScore.ToString(CountFormat, culture);
            lblCardsInStockValue.Text = _table.StockCount.ToString(CountFormat, culture);
            if (_handResult != null)
            {
                RefreshHandResult(culture);
            }

            if (_matchResult != null)
            {
                lblFinalScore.Text = Localization.Format(FinalScoreKey, _matchResult.PlayerScore, _matchResult.OpponentScore);
            }
        }

        private void OnScreenLoaded(object sender, RoutedEventArgs e)
        {
            ScrollToLatest(lstMessages);
            ScrollToLatest(lstMatchLog);
        }

        private void OnTakeClick(object sender, RoutedEventArgs e)
        {
            // The card turned up goes to the hand, which then owes a discard. The server checks
            // the move and passes the turn (house rules 2 and 3).
            CardDto takenCard = _discardPile[0];
            _discardPile.Clear();
            _hand.Add(takenCard);
            EndDecision();
        }

        private void OnPassClick(object sender, RoutedEventArgs e)
        {
            AddLogEntry(MatchLogKind.Passed);
            EndDecision();
        }

        private void OnHandPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(lstHand);
            _pressedCard = GetCard(e.OriginalSource);
        }

        private void OnHandPreviewMouseMove(object sender, MouseEventArgs e)
        {
            // A press that barely moves is a click that chooses the card; only a longer move
            // picks it up to reorder the hand (house rule 18).
            if (e.LeftButton == MouseButtonState.Pressed && _pressedCard != null
                && HasLeftClickArea(e.GetPosition(lstHand)))
            {
                CardDto draggedCard = _pressedCard;
                _pressedCard = null;
                DragDrop.DoDragDrop(lstHand, draggedCard, DragDropEffects.Move);
            }
        }

        private void OnHandDrop(object sender, DragEventArgs e)
        {
            CardDto draggedCard = e.Data.GetData(typeof(CardDto)) as CardDto;
            CardDto targetCard = GetCard(e.OriginalSource);
            if (draggedCard != null && targetCard != null && draggedCard != targetCard)
            {
                _hand.Move(_hand.IndexOf(draggedCard), _hand.IndexOf(targetCard));
                lstHand.SelectedItem = draggedCard;
            }
        }

        private void OnSendClick(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void OnMessageKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            // The rules open over the table without pausing the match or changing the turn
            // (CU-21 FA-01).
            GuiHouseRules houseRules = new GuiHouseRules();
            houseRules.Owner = this;
            houseRules.ShowDialog();
        }

        private void OnForfeitClick(object sender, RoutedEventArgs e)
        {
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.Forfeit);
            confirmDialog.Owner = this;
            confirmDialog.ShowDialog();

            // The server records the defeat (CU-26 steps 5 to 7); what the table does is return
            // the player to the lobby, as step 9 does. Cancelling leaves the match as it was.
            if (confirmDialog.IsConfirmed)
            {
                ReturnToLobby();
            }
        }

        private void OnNextHandClick(object sender, RoutedEventArgs e)
        {
            // The score of the match takes the count of the hand, and the table waits for the
            // server to deal the next one.
            _table.PlayerScore = _handResult.PlayerScore;
            _table.OpponentScore = _handResult.OpponentScore;
            lblKnockWith.Visibility = Visibility.Collapsed;
            RefreshFormattedText();
        }

        private void OnReturnToLobbyClick(object sender, RoutedEventArgs e)
        {
            ReturnToLobby();
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            Loaded -= OnScreenLoaded;
            Closed -= OnScreenClosed;
        }

        private void RefreshHandResult(CultureInfo culture)
        {
            // The names inside the sentences are data and keep their case; the sentences carry
            // the case of the prototype in the dictionary itself.
            lblHandWinner.Text = Localization.Format(HandWinnerKey, _handResult.WinnerName);
            lblKnockWith.Text = Localization.Format(KnockWithKey, _handResult.KnockerDeadwood);
            lblKnockAnnounce.Text = Localization.Format(KnockAnnounceKey, _handResult.KnockerName, _handResult.KnockerDeadwood);
            lblYourDeadwoodValue.Text = _handResult.DefenderDeadwood.ToString(CountFormat, culture);
            lblMinusOpponentDeadwood.Text = Localization.Format(MinusOpponentDeadwoodKey, _handResult.KnockerName);
            lblOpponentDeadwoodValue.Text = _handResult.KnockerDeadwood.ToString(SubtractedFormat, culture);
            lblPointsFor.Text = Localization.Format(PointsForKey, _handResult.WinnerName);
            lblPointsForValue.Text = _handResult.PointsAwarded.ToString(EarnedFormat, culture);
            lblHandRoleKnocked.Text = Localization.Format(HandRoleKnockedKey, _handResult.KnockerName);
            lblDeadwoodCount.Text = Localization.Format(DeadwoodCountKey, _handResult.KnockerDeadwood);
            lblDefenderDeadwoodCount.Text = Localization.Format(DeadwoodCountKey, _handResult.DefenderDeadwood);
            lblScoreTarget.Text = Localization.Format(ScoreTargetKey, _table.TargetScore);
            lblHandPlayerScore.Text = _handResult.PlayerScore.ToString(CountFormat, culture);
            lblHandOpponentScore.Text = _handResult.OpponentScore.ToString(CountFormat, culture);
        }

        private void EndDecision()
        {
            // Hidden and not collapsed, so that the table keeps its shape while it waits for the
            // opponent.
            btnTake.Visibility = Visibility.Hidden;
            btnPass.Visibility = Visibility.Hidden;
            RefreshFormattedText();
        }

        private void SendMessage()
        {
            string content = txtMessage.Text.Trim();

            // As in the lobby, an empty message is not sent (CU-17 FA-03), and the server relays
            // the rest; until it answers, the table shows the message of the player.
            if (content.Length > 0)
            {
                ChatMessageDto message = new ChatMessageDto();
                message.AuthorName = _table.PlayerName;
                message.Content = content;
                message.SentAt = DateTime.Now;
                _chatEntries.Add(message);
                txtMessage.Clear();
                ScrollToLatest(lstMessages);
            }
        }

        private void AddLogEntry(MatchLogKind kind)
        {
            MatchLogEntryDto entry = new MatchLogEntryDto();
            entry.PlayerName = _table.PlayerName;
            entry.OccurredAt = DateTime.Now;
            entry.Kind = kind;
            _matchLog.Add(entry);
            ScrollToLatest(lstMatchLog);
        }

        private void ReturnToLobby()
        {
            ReplaceInPlace(new GuiLobbyChat());
        }

        private string FormatCardCount(int count)
        {
            // Spanish and English only change the word for a single card, but the dictionary
            // keeps it as a key of its own so that a language with more forms can add them.
            string key = count == 1 ? CardCountOneKey : CardCountKey;

            return Localization.Format(key, count).ToUpper(Localization.CurrentCulture);
        }

        private bool HasLeftClickArea(Point position)
        {
            Vector distance = position - _dragStart;

            return Math.Abs(distance.X) > SystemParameters.MinimumHorizontalDragDistance
                || Math.Abs(distance.Y) > SystemParameters.MinimumVerticalDragDistance;
        }

        private static CardDto GetCard(object source)
        {
            FrameworkElement element = source as FrameworkElement;

            return element != null ? element.DataContext as CardDto : null;
        }

        private static void ScrollToLatest(ListBox list)
        {
            if (list.Items.Count > 0)
            {
                list.ScrollIntoView(list.Items[list.Items.Count - 1]);
            }
        }
    }
}
