using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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

        private const string MeldRunKey = "GameTable_LblMeldRun";
        private const string MeldThreeKey = "GameTable_LblMeldThree";
        private const string MeldFourKey = "GameTable_LblMeldFour";
        private const string MeldCaptionStyleKey = "StyMeldCaption";
        private const string MeldBarStyleKey = "StyMeldBar";
        private const int FourOfAKindCount = 4;
        private const double MeldBarGap = 6.0;
        private const double MeldCaptionGap = 2.0;

        // Melds that sit side by side would draw a single bar, so each bar is drawn a little shorter than its cards.
        private const double MeldBarInset = 3.0;

        private readonly GameTableViewModel _viewModel;

        public GuiGameTable(GameTableViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.ChatMessages.CollectionChanged += OnChatMessagesChanged;
            _viewModel.MatchLog.CollectionChanged += OnMatchLogChanged;
            _viewModel.CardDrawnFromStock += OnCardDrawnFromStock;
            _viewModel.CardTakenFromDiscardPile += OnCardTakenFromDiscardPile;
            _viewModel.Hand.CollectionChanged += OnHandChanged;
            _viewModel.PropertyChanged += OnTableChanged;
            lstStock.ItemsSource = _stockTopCard;
            DataContext = _viewModel;
            Loaded += OnScreenLoaded;
            Closed += OnScreenClosed;
        }

        private void OnScreenLoaded(object sender, RoutedEventArgs e)
        {
            ScrollToLatest(lstMessages);
            ScrollToLatest(lstMatchLog);
            CompositionTarget.Rendering += OnRendering;
        }

        protected override void RefreshFormattedText()
        {
            ShowMeldCaptions();
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
            _viewModel.Hand.CollectionChanged -= OnHandChanged;
            _viewModel.PropertyChanged -= OnTableChanged;
            CompositionTarget.Rendering -= OnRendering;
            Loaded -= OnScreenLoaded;
            Closed -= OnScreenClosed;
        }

        private void OnHandChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ShowMeldCaptions();
        }

        // The table raises an empty name when a new state of the round arrives, and only then can the melds change.
        private void OnTableChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName))
            {
                ShowMeldCaptions();
            }
        }

        // The captions are built again only when the melds or the order of the hand change, and each frame just moves them.
        private void ShowMeldCaptions()
        {
            cnvMeldCaptions.Children.Clear();
            foreach (Meld meld in _viewModel.HandMelds)
            {
                string caption = Localization[GetMeldCaptionKey(meld)];
                foreach (IList<Card> stretch in HandOrder.SplitIntoNeighbors(_viewModel.Hand, meld.Cards))
                {
                    AddMeldCaption(caption, stretch);
                }
            }
        }

        // Both parts stay hidden until the first frame places them over their cards.
        private void AddMeldCaption(string caption, IList<Card> stretch)
        {
            TextBlock name = new TextBlock();
            name.Style = (Style)FindResource(MeldCaptionStyleKey);
            name.Text = caption;
            name.Tag = stretch;
            name.Visibility = Visibility.Hidden;
            Rectangle bar = new Rectangle();
            bar.Style = (Style)FindResource(MeldBarStyleKey);
            bar.Tag = stretch;
            bar.Visibility = Visibility.Hidden;
            cnvMeldCaptions.Children.Add(name);
            cnvMeldCaptions.Children.Add(bar);
        }

        // The hand animates its cards on every frame, so the captions follow them on every frame too.
        // The bar keeps the height of the top of the hand, so a card lifted by the player does not move it.
        private void OnRendering(object sender, EventArgs e)
        {
            double barTop = lstHand.TranslatePoint(new Point(0, lstHand.TiltRoom), cnvMeldCaptions).Y - MeldBarGap;
            foreach (FrameworkElement caption in cnvMeldCaptions.Children)
            {
                Rect bounds = GetStretchBounds((IList<Card>)caption.Tag);
                caption.Visibility = bounds.IsEmpty ? Visibility.Hidden : Visibility.Visible;
                if (!bounds.IsEmpty)
                {
                    PlaceMeldCaption(caption, bounds, barTop);
                }
            }
        }

        private Rect GetStretchBounds(IList<Card> stretch)
        {
            Rect bounds = Rect.Empty;
            foreach (Card card in stretch)
            {
                Border host = lstHand.ItemContainerGenerator.ContainerFromItem(card) as Border;
                if ((host != null) && (host.Child is FrameworkElement faces))
                {
                    bounds.Union(faces.TransformToVisual(cnvMeldCaptions).TransformBounds(new Rect(faces.RenderSize)));
                }
            }

            return bounds;
        }

        private static void PlaceMeldCaption(FrameworkElement caption, Rect bounds, double barTop)
        {
            if (caption is Rectangle)
            {
                caption.Width = Math.Max(0, bounds.Width - (2 * MeldBarInset));
                Canvas.SetLeft(caption, bounds.Left + MeldBarInset);
                Canvas.SetTop(caption, barTop);
            }
            else
            {
                Canvas.SetLeft(caption, bounds.Left + ((bounds.Width - caption.ActualWidth) / 2));
                Canvas.SetTop(caption, barTop - MeldCaptionGap - caption.ActualHeight);
            }
        }

        private static string GetMeldCaptionKey(Meld meld)
        {
            string key = MeldRunKey;
            if (meld.Kind == MeldKind.Set)
            {
                key = (meld.Cards.Count == FourOfAKindCount) ? MeldFourKey : MeldThreeKey;
            }

            return key;
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
