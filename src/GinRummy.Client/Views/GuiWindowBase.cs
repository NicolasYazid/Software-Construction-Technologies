using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

using GinRummy.Client.Localization;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Base window of the main screens: the main menu, the lobbies and the match. It keeps the
    /// window subscribed to the culture change, hosts the modals of the screen inside the
    /// window itself, over a shade that dims the screen, and places the main screens: the
    /// lobby in place of the menu and the match over the lobby.
    /// </summary>
    public class GuiWindowBase : Window
    {
        private const string BackdropBrushKey = "BrsModalBackdrop";
        private const string ShadowEffectKey = "EfxPanelShadow";
        private const string CloseButtonStyleKey = "StyDialogCloseButton";
        private const string CloseIconKey = "IcoClose";
        private const double CloseIconSize = 24.0;
        private const double CloseButtonInset = 14.0;

        private readonly LocalizationProvider _localization;
        private readonly List<GuiModalBase> _modals;
        private Grid _modalLayer;
        private bool _returnsToMenu;
        private bool _isHandingOver;
        private bool _isClosed;

        /// <summary>
        /// Subscribes the window to the culture change and to the key that closes a modal.
        /// </summary>
        protected GuiWindowBase()
        {
            _localization = LocalizationProvider.Instance;
            _localization.PropertyChanged += OnLocalizationChanged;
            _modals = new List<GuiModalBase>();
            Closed += OnWindowClosed;
            PreviewKeyDown += OnWindowPreviewKeyDown;
        }

        /// <summary>
        /// Gets the localization provider shared by the whole application.
        /// </summary>
        protected LocalizationProvider Localization
        {
            get { return _localization; }
        }

        /// <summary>
        /// Opens a modal inside this window, over the screen and over any modal already open.
        /// The screen is dimmed behind it and does not take the mouse until the modal closes.
        /// </summary>
        /// <param name="modal">Modal to open.</param>
        public void ShowModal(GuiModalBase modal)
        {
            Border backdrop = new Border();
            backdrop.Background = (Brush)FindResource(BackdropBrushKey);
            Grid entry = new Grid();
            entry.Children.Add(backdrop);
            entry.Children.Add(BuildFrame(modal));
            _modalLayer.Children.Add(entry);
            _modals.Add(modal);
            modal.AttachTo(this, entry);
        }

        /// <summary>
        /// Opens a lobby in place of the main menu, which hides until the lobby closes. The
        /// modals open on this window close, and so does this window unless it is the menu.
        /// </summary>
        /// <param name="lobby">Lobby the player enters.</param>
        public void EnterLobby(GuiWindowBase lobby)
        {
            Window mainMenu = Application.Current.MainWindow;
            CloseAllModals();
            lobby._returnsToMenu = true;
            lobby.Show();
            mainMenu.Hide();

            if (!ReferenceEquals(this, mainMenu))
            {
                _isHandingOver = true;
                Close();
            }
        }

        /// <summary>
        /// Opens the next modal of a flow in the place of the current one.
        /// </summary>
        /// <param name="current">Modal that leaves.</param>
        /// <param name="nextScreen">Modal that takes its place.</param>
        internal void ReplaceModal(GuiModalBase current, GuiModalBase nextScreen)
        {
            ShowModal(nextScreen);
            current.Close();
        }

        /// <summary>
        /// Takes a closed modal off the window.
        /// </summary>
        /// <param name="modal">Modal that closed.</param>
        internal void RemoveModal(GuiModalBase modal)
        {
            _modalLayer.Children.Remove(modal.Entry);
            _modals.Remove(modal);
        }

        /// <summary>
        /// Lays the layer of the modals over the content of the window once the window has
        /// read its content, so the screens need no markup of their own for it.
        /// </summary>
        /// <param name="e">Arguments of the event.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            UIElement screen = Content as UIElement;
            Content = null;
            Grid root = new Grid();
            root.Children.Add(screen);
            _modalLayer = new Grid();
            root.Children.Add(_modalLayer);
            Content = root;
        }

        /// <summary>
        /// Rebuilds the texts that are assembled from a format string. Screens that show
        /// counters, durations or interpolated values override it.
        /// </summary>
        protected virtual void RefreshFormattedText()
        {
        }

        /// <summary>
        /// Opens a main screen over this one, which stays visible beneath it, as the match
        /// does over the lobby.
        /// </summary>
        /// <param name="nextScreen">Screen that opens over this one.</param>
        protected void OpenOver(GuiWindowBase nextScreen)
        {
            nextScreen.Owner = this;
            nextScreen.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            nextScreen.Show();
        }

        /// <summary>
        /// Leaves this screen for the main menu and opens a modal over it, as a guest does
        /// when it decides to sign in from its lobby.
        /// </summary>
        /// <param name="nextScreen">Modal that opens over the main menu.</param>
        protected void ReturnToMenuWith(GuiModalBase nextScreen)
        {
            GuiWindowBase mainMenu = Application.Current.MainWindow as GuiWindowBase;
            _isHandingOver = true;
            ShowMainMenu();
            if (mainMenu != null)
            {
                mainMenu.ShowModal(nextScreen);
            }

            Close();
        }

        // The main menu is the main window of the application, so it is found there and not
        // passed from screen to screen.
        private static void ShowMainMenu()
        {
            GuiWindowBase mainMenu = Application.Current.MainWindow as GuiWindowBase;
            if (mainMenu != null && !mainMenu._isClosed)
            {
                mainMenu.Show();
                mainMenu.Activate();
            }
        }

        // The modal sits in a frame of its own size that carries the shadow and, for the
        // modals without a way to close of their own, the closing cross in its corner.
        private Grid BuildFrame(GuiModalBase modal)
        {
            Grid frame = new Grid();
            frame.HorizontalAlignment = modal.HorizontalAlignment;
            frame.VerticalAlignment = modal.VerticalAlignment;
            frame.Effect = (Effect)FindResource(ShadowEffectKey);
            frame.Children.Add(modal);
            if (modal.ShowsCloseButton)
            {
                frame.Children.Add(BuildCloseButton(modal));
            }

            return frame;
        }

        private Button BuildCloseButton(GuiModalBase modal)
        {
            Image icon = new Image();
            icon.Source = (ImageSource)FindResource(CloseIconKey);
            icon.Width = CloseIconSize;
            icon.Height = CloseIconSize;
            Button closeButton = new Button();
            closeButton.Style = (Style)FindResource(CloseButtonStyleKey);
            closeButton.Content = icon;
            closeButton.HorizontalAlignment = HorizontalAlignment.Right;
            closeButton.VerticalAlignment = VerticalAlignment.Top;
            closeButton.Margin = new Thickness(0, CloseButtonInset, CloseButtonInset, 0);
            closeButton.Click += (sender, e) => modal.Close();

            return closeButton;
        }

        private void CloseAllModals()
        {
            List<GuiModalBase> openModals = new List<GuiModalBase>(_modals);
            openModals.Reverse();
            foreach (GuiModalBase modal in openModals)
            {
                modal.Close();
            }
        }

        private void OnLocalizationChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshFormattedText();
        }

        // Escape closes the modal on top, as the cross of a window used to.
        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _modals.Count > 0)
            {
                _modals[_modals.Count - 1].Close();
                e.Handled = true;
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            _isClosed = true;
            CloseAllModals();
            _localization.PropertyChanged -= OnLocalizationChanged;
            PreviewKeyDown -= OnWindowPreviewKeyDown;
            Closed -= OnWindowClosed;

            // A lobby that closes without handing over to another screen, because the player
            // left it, gives its place back to the main menu.
            if (_returnsToMenu && !_isHandingOver)
            {
                ShowMainMenu();
            }
        }
    }
}
