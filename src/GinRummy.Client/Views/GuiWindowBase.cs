using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

using GinRummy.Client.Controls;
using GinRummy.Client.Localization;

namespace GinRummy.Client.Views
{
    // Modals open inside the window over a dimming shade instead of in windows of their own.
    // The lobby takes the place of the main menu, while the match opens over the lobby.
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
        private UIElement _screen;
        private Grid _modalLayer;
        private bool _isInPlaceOfMenu;
        private bool _isHandingOver;
        private bool _isClosed;

        protected GuiWindowBase()
        {
            _localization = LocalizationProvider.Instance;
            _localization.PropertyChanged += OnLocalizationChanged;
            _modals = new List<GuiModalBase>();
            Closed += OnWindowClosed;
            PreviewKeyDown += OnWindowPreviewKeyDown;
        }

        protected LocalizationProvider Localization
        {
            get { return _localization; }
        }

        public void ShowModal(GuiModalBase modal)
        {
            if (_modals.Count == 0)
            {
                PausePaints(_screen, true);
            }

            Border backdrop = new Border();
            backdrop.Background = (Brush)FindResource(BackdropBrushKey);
            Grid entry = new Grid();
            entry.Children.Add(backdrop);
            entry.Children.Add(BuildFrame(modal));
            _modalLayer.Children.Add(entry);
            _modals.Add(modal);
            modal.AttachTo(this, entry);
        }

        public void EnterLobby(GuiWindowBase lobby)
        {
            Window mainMenu = Application.Current.MainWindow;
            CloseAllModals();
            lobby._isInPlaceOfMenu = true;
            lobby.Show();
            mainMenu.Hide();

            if (!ReferenceEquals(this, mainMenu))
            {
                _isHandingOver = true;
                Close();
            }
        }

        internal void ReplaceModal(GuiModalBase current, GuiModalBase nextScreen)
        {
            ShowModal(nextScreen);
            current.Close();
        }

        internal void RemoveModal(GuiModalBase modal)
        {
            _modalLayer.Children.Remove(modal.Entry);
            _modals.Remove(modal);
            if (_modals.Count == 0)
            {
                PausePaints(_screen, false);
            }
        }

        // The modal layer is added in code once WPF has loaded the content, so the screens need no markup of their own for it.
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _screen = Content as UIElement;
            Content = null;
            Grid root = new Grid();
            root.Children.Add(_screen);
            _modalLayer = new Grid();
            root.Children.Add(_modalLayer);
            Content = root;
        }

        protected virtual void RefreshFormattedText()
        {
        }

        protected void OpenOver(GuiWindowBase nextScreen)
        {
            nextScreen.Owner = this;
            nextScreen.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            PausePaints(this, true);
            nextScreen.Closed += (sender, e) => PausePaints(this, false);
            nextScreen.Show();
        }

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

        // The main menu is looked up as the application main window instead of being passed from screen to screen.
        private static void ShowMainMenu()
        {
            GuiWindowBase mainMenu = Application.Current.MainWindow as GuiWindowBase;
            if ((mainMenu != null) && !mainMenu._isClosed)
            {
                mainMenu.Show();
                mainMenu.Activate();
            }
        }

        // A covered screen pauses its paint while a modal or another screen is over it, so two paints never animate at the same time.
        private static void PausePaints(DependencyObject root, bool isPaused)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(root);
            for (int childIndex = 0; childIndex < childCount; childIndex++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, childIndex);
                PaintedBackground paint = child as PaintedBackground;
                if (paint != null)
                {
                    paint.IsPaused = isPaused;
                }
                else
                {
                    PausePaints(child, isPaused);
                }
            }
        }

        private Grid BuildFrame(GuiModalBase modal)
        {
            Grid frame = new Grid();
            frame.HorizontalAlignment = modal.HorizontalAlignment;
            frame.VerticalAlignment = modal.VerticalAlignment;
            frame.Effect = (Effect)FindResource(ShadowEffectKey);
            frame.Children.Add(modal);
            if (modal.HasCloseButton)
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

        // Escape closes the top modal to keep the behavior modals had when they were windows of their own.
        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Escape) && (_modals.Count > 0))
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

            // A lobby the player closes, instead of handing it over to another screen, gives its place back to the main menu.
            if (_isInPlaceOfMenu && !_isHandingOver)
            {
                ShowMainMenu();
            }
        }
    }
}
