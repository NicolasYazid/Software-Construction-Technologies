using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

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
        private readonly GameTableViewModel _viewModel;
        private Point _dragStart;
        private Card _pressedCard;

        public GuiGameTable()
        {
            InitializeComponent();
            _viewModel = new GameTableViewModel(((App)Application.Current).SampleData);
            _viewModel.ChatMessages.CollectionChanged += OnChatMessagesChanged;
            _viewModel.MatchLog.CollectionChanged += OnMatchLogChanged;
            DataContext = _viewModel;
            Loaded += OnScreenLoaded;
            Closed += OnScreenClosed;
        }

        private void OnScreenLoaded(object sender, RoutedEventArgs e)
        {
            ScrollToLatest(lstMessages);
            ScrollToLatest(lstMatchLog);
        }

        private void OnChatMessagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ScrollToLatest(lstMessages);
        }

        private void OnMatchLogChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ScrollToLatest(lstMatchLog);
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
                Card draggedCard = _pressedCard;
                _pressedCard = null;
                DragDrop.DoDragDrop(lstHand, draggedCard, DragDropEffects.Move);
            }
        }

        private void OnHandDrop(object sender, DragEventArgs e)
        {
            Card draggedCard = e.Data.GetData(typeof(Card)) as Card;
            Card targetCard = GetCard(e.OriginalSource);
            bool isDroppedOnCard = (draggedCard != null) && (targetCard != null);
            if (isDroppedOnCard && (draggedCard != targetCard))
            {
                _viewModel.MoveCard(draggedCard, targetCard);
                lstHand.SelectedItem = draggedCard;
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
                Close();
            }
        }

        private void OnReturnToLobbyClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.ChatMessages.CollectionChanged -= OnChatMessagesChanged;
            _viewModel.MatchLog.CollectionChanged -= OnMatchLogChanged;
            Loaded -= OnScreenLoaded;
            Closed -= OnScreenClosed;
        }

        private bool HasLeftClickArea(Point position)
        {
            Vector distance = position - _dragStart;
            bool hasLeftClickArea = (Math.Abs(distance.X) > SystemParameters.MinimumHorizontalDragDistance)
                || (Math.Abs(distance.Y) > SystemParameters.MinimumVerticalDragDistance);

            return hasLeftClickArea;
        }

        private static Card GetCard(object source)
        {
            FrameworkElement element = source as FrameworkElement;
            Card card = null;
            if (element != null)
            {
                card = element.DataContext as Card;
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
