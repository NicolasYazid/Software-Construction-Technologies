using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

using GinRummy.Client.Controls;
using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Game table screen (P20). Shows a match from the side of one of its players: the hand,
    /// what can be seen of the opponent, the stock and the discard pile, the score, the log of
    /// the match and its chat (CU-17 FA-01). From it the player reads the rules (CU-21 FA-01)
    /// and forfeits the match (CU-26). Over it open the close of the hand and the result of the match.
    /// </summary>
    public partial class GuiGameTable : GuiWindowBase
    {
        // The stock shows only its top card, face down, so it holds a single placeholder item.
        private static readonly int[] _stockTopCard = { 0 };

        private readonly GameTableViewModel _viewModel;

        public GuiGameTable(GameTableViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.ChatMessages.CollectionChanged += OnChatMessagesChanged;
            _viewModel.MatchLog.CollectionChanged += OnMatchLogChanged;
            _viewModel.CardDrawnFromStock += OnCardDrawnFromStock;
            _viewModel.CardTakenFromDiscardPile += OnCardTakenFromDiscardPile;
            lstStock.ItemsSource = _stockTopCard;
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

        private void OnCardDrawnFromStock(object sender, Card drawnCard)
        {
            lstHand.EnterFrom(drawnCard, lstStock, true);
        }

        // A card released over the hand goes on from where it was dropped instead of starting again from the pile.
        private void OnCardTakenFromDiscardPile(object sender, Card takenCard)
        {
            lstHand.EnterFrom(takenCard, lstDiscard);
        }

        private void OnHandCardMoved(object sender, CardMovedEventArgs e)
        {
            _viewModel.MoveCard(e.OldIndex, e.NewIndex);
        }

        // A card released over the pile at any other step of the turn goes back to the hand.
        private void OnHandCardDropped(object sender, CardEventArgs e)
        {
            if (_viewModel.IsDiscarding)
            {
                lstHand.SendTo(e.Item, lstDiscard);
            }
        }

        // The discard is sent once the card lands, so the pile never shows it before it arrives.
        private void OnHandCardLeft(object sender, CardEventArgs e)
        {
            _viewModel.Discard((Card)e.Item);
        }

        private void OnDiscardCardDropped(object sender, CardEventArgs e)
        {
            _viewModel.TakeFromDiscardPile();
        }

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
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
            // The table only returns the player to the lobby (CU-26 step 9)
            // because the server gives the round to the opponent (CU-26 steps 5 to 7).
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
            _viewModel.Stop();
            _viewModel.ChatMessages.CollectionChanged -= OnChatMessagesChanged;
            _viewModel.MatchLog.CollectionChanged -= OnMatchLogChanged;
            _viewModel.CardDrawnFromStock -= OnCardDrawnFromStock;
            _viewModel.CardTakenFromDiscardPile -= OnCardTakenFromDiscardPile;
            Loaded -= OnScreenLoaded;
            Closed -= OnScreenClosed;
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
