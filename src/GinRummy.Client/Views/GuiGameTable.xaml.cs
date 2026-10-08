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
        private const int SingleCardCount = 1;

        private readonly GameTableSnapshotDto _table;
        private readonly ObservableCollection<CardDto> _hand;
        private readonly ObservableCollection<CardDto> _discardPile;
        private readonly ObservableCollection<ChatMessageDto> _chatEntries;
        private readonly ObservableCollection<MatchLogEntryDto> _matchLog;
        private HandResultDto _handResult;
        private MatchResultDto _matchResult;
        private Point _dragStart;
        private CardDto _pressedCard;

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

        // The match resumes when the disconnected opponent returns, or ends if the opponent does not (BD-05).
        public void ShowPaused()
        {
            lblPaused.Visibility = Visibility.Visible;
        }

        public void HidePaused()
        {
            lblPaused.Visibility = Visibility.Collapsed;
        }

        public void ShowHandResult(HandResultDto handResult)
        {
            _handResult = handResult;
            lstKnockerMelds.ItemsSource = handResult.KnockerMelds;
            lstDefenderMelds.ItemsSource = handResult.DefenderMelds;
            lblKnockWith.Visibility = Visibility.Visible;
            RefreshFormattedText();
        }

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
            // The move is not validated here because the server checks it and passes the turn (house rules 2 and 3).
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
            // The drag waits for the system drag distance so that a short press still chooses the card (house rule 18).
            bool isCardPressed = (e.LeftButton == MouseButtonState.Pressed) && (_pressedCard != null);
            if (isCardPressed && HasLeftClickArea(e.GetPosition(lstHand)))
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
            bool isDroppedOnCard = (draggedCard != null) && (targetCard != null);
            if (isDroppedOnCard && (draggedCard != targetCard))
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
            // The rules open over the table without pausing the match or changing the turn (CU-21 FA-01).
            GuiHouseRules houseRules = new GuiHouseRules();
            ShowModal(houseRules);
        }

        private void OnForfeitClick(object sender, RoutedEventArgs e)
        {
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.Forfeit);
            confirmDialog.Closed += OnForfeitConfirmClosed;
            ShowModal(confirmDialog);
        }

        private void OnForfeitConfirmClosed(object sender, EventArgs e)
        {
            // The table only returns the player to the lobby (CU-26 step 9) because the server records the defeat (CU-26 steps 5 to 7).
            if (((GuiConfirmDialog)sender).IsConfirmed)
            {
                ReturnToLobby();
            }
        }

        private void OnNextHandClick(object sender, RoutedEventArgs e)
        {
            // The table only carries the score of the hand into the match because the server deals the next hand.
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
            // These sentences are not converted to uppercase because the names inside them are data and keep their case.
            // The dictionary itself carries the case of the prototype for the rest of each sentence.
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
            // The buttons are hidden and not collapsed, so that the table keeps its shape while it waits for the opponent.
            btnTake.Visibility = Visibility.Hidden;
            btnPass.Visibility = Visibility.Hidden;
            RefreshFormattedText();
        }

        private void SendMessage()
        {
            string content = txtMessage.Text.Trim();

            // As in the lobby, an empty message is not sent (CU-17 FA-03).
            // The server relays the rest, and until it answers the table shows the message of the player.
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
            Close();
        }

        private string FormatCardCount(int count)
        {
            // Spanish and English only change the word for a single card.
            // The dictionary keeps that form as a key of its own so that a language with more forms can add them.
            string key = count == SingleCardCount ? CardCountOneKey : CardCountKey;
            string formattedCount = Localization.Format(key, count).ToUpper(Localization.CurrentCulture);

            return formattedCount;
        }

        private bool HasLeftClickArea(Point position)
        {
            Vector distance = position - _dragStart;
            bool hasLeftClickArea = (Math.Abs(distance.X) > SystemParameters.MinimumHorizontalDragDistance)
                || (Math.Abs(distance.Y) > SystemParameters.MinimumVerticalDragDistance);

            return hasLeftClickArea;
        }

        private static CardDto GetCard(object source)
        {
            FrameworkElement element = source as FrameworkElement;
            CardDto card = null;
            if (element != null)
            {
                card = element.DataContext as CardDto;
            }

            return card;
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
