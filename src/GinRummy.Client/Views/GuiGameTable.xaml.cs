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
        private static readonly int[] s_stockTopCard = { 0 };

        private const string MeldRunKey = "GameTable_LblMeldRun";
        private const string MeldThreeKey = "GameTable_LblMeldThree";
        private const string MeldFourKey = "GameTable_LblMeldFour";
        private const string MeldCaptionStyleKey = "StyMeldCaption";
        private const string MeldBarStyleKey = "StyMeldBar";
        private const int FourOfAKindCount = 4;
        private const double MeldBarGap = 6.0;
        private const double MeldCaptionGap = 2.0;

        private const double MeldBarInset = 3.0;

        private readonly GameTableViewModel _viewModel;
        private GuiReconnecting _reconnecting;

        public GuiGameTable(GameTableViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.ChatMessages.CollectionChanged += HandleChatMessagesChanged;
            _viewModel.MatchLog.CollectionChanged += HandleMatchLogChanged;
            _viewModel.CardDrawnFromStock += HandleCardDrawnFromStock;
            _viewModel.CardTakenFromDiscardPile += HandleCardTakenFromDiscardPile;
            _viewModel.Hand.CollectionChanged += HandleHandChanged;
            _viewModel.PropertyChanged += HandleTableChanged;
            _viewModel.ConnectionLost += HandleConnectionLost;
            _viewModel.SessionExpired += HandleSessionExpired;
            _viewModel.MatchAlreadyOver += HandleMatchAlreadyOver;
            lstStock.ItemsSource = s_stockTopCard;
            DataContext = _viewModel;
            Loaded += HandleScreenLoaded;
            Closed += HandleScreenClosed;
        }

        private void HandleScreenLoaded(object sender, RoutedEventArgs e)
        {
            OverlayInsetCommon.ScrollToEnd(lstMessages);
            OverlayInsetCommon.ScrollToEnd(lstMatchLog);
            CompositionTarget.Rendering += HandleRendering;
        }

        protected override void RefreshFormattedText()
        {
            ShowMeldCaptions();
        }

        private void HandleChatMessagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OverlayInsetCommon.ScrollToEnd(lstMessages);
        }

        private void HandleMatchLogChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OverlayInsetCommon.ScrollToEnd(lstMatchLog);
        }

        private void HandleCardDrawnFromStock(object sender, Card drawnCard)
        {
            lstHand.EnterFrom(drawnCard, lstStock, true);
        }

        private void HandleCardTakenFromDiscardPile(object sender, Card takenCard)
        {
            lstHand.EnterFrom(takenCard, lstDiscard);
        }

        private void HandleHandCardMoved(object sender, CardMovedEventArgs e)
        {
            _viewModel.MoveCard(e.OldIndex, e.NewIndex);
        }

        private void HandleHandCardDropped(object sender, CardEventArgs e)
        {
            if (_viewModel.CanDiscard((Card)e.Item))
            {
                lstHand.SendTo(e.Item, lstDiscard);
            }
        }

        private void HandleHandCardLeft(object sender, CardEventArgs e)
        {
            _viewModel.Discard((Card)e.Item);
        }

        private void HandleDiscardCardDropped(object sender, CardEventArgs e)
        {
            _viewModel.TakeFromDiscardPile();
        }

        private void HandleHowToPlayClick(object sender, RoutedEventArgs e)
        {
            GuiHouseRules houseRules = new GuiHouseRules();
            ShowModal(houseRules);
        }

        private void HandleForfeitClick(object sender, RoutedEventArgs e)
        {
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.Forfeit);
            confirmDialog.Closed += HandleForfeitConfirmClosed;
            ShowModal(confirmDialog);
        }

        private void HandleForfeitConfirmClosed(object sender, EventArgs e)
        {
            if (((GuiConfirmDialog)sender).IsConfirmed)
            {
                Close();
            }
        }

        private void HandleConnectionLost(object sender, EventArgs e)
        {
            _reconnecting = new GuiReconnecting(_viewModel.Reconnect);
            _reconnecting.Closed += HandleReconnectingClosed;
            ShowModal(_reconnecting);
        }

        private void HandleMatchAlreadyOver(object sender, EventArgs e)
        {
            _reconnecting.Closed -= HandleReconnectingClosed;
            _reconnecting.Close();
            GuiErrorDialog errorDialog = new GuiErrorDialog(ErrorDialogKind.MatchFinished);
            errorDialog.Closed += HandleMatchFinishedDialogClosed;
            ShowModal(errorDialog);
        }

        private void HandleMatchFinishedDialogClosed(object sender, EventArgs e)
        {
            Close();
        }

        private void HandleReconnectingClosed(object sender, EventArgs e)
        {
            if (!((GuiReconnecting)sender).IsReconnected)
            {
                Close();
            }
        }

        private void HandleSessionExpired(object sender, EventArgs e)
        {
            GuiErrorDialog errorDialog = new GuiErrorDialog(ErrorDialogKind.SessionExpired);
            errorDialog.Closed += HandleSessionExpiredDialogClosed;
            ShowModal(errorDialog);
        }

        private void HandleSessionExpiredDialogClosed(object sender, EventArgs e)
        {
            Window lobby = Owner;
            Close();
            lobby.Close();
            ((App)Application.Current).ForgetSignedInPlayer();
        }

        private void HandleReturnToLobbyClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HandleScreenClosed(object sender, EventArgs e)
        {
            _viewModel.Stop();
            _viewModel.ChatMessages.CollectionChanged -= HandleChatMessagesChanged;
            _viewModel.MatchLog.CollectionChanged -= HandleMatchLogChanged;
            _viewModel.CardDrawnFromStock -= HandleCardDrawnFromStock;
            _viewModel.CardTakenFromDiscardPile -= HandleCardTakenFromDiscardPile;
            _viewModel.Hand.CollectionChanged -= HandleHandChanged;
            _viewModel.PropertyChanged -= HandleTableChanged;
            _viewModel.ConnectionLost -= HandleConnectionLost;
            _viewModel.SessionExpired -= HandleSessionExpired;
            _viewModel.MatchAlreadyOver -= HandleMatchAlreadyOver;
            CompositionTarget.Rendering -= HandleRendering;
            Loaded -= HandleScreenLoaded;
            Closed -= HandleScreenClosed;
        }

        private void HandleHandChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ShowMeldCaptions();
        }

        private void HandleTableChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName))
            {
                ShowMeldCaptions();
            }
        }

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

        private void HandleRendering(object sender, EventArgs e)
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
    }
}
