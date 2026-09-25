using System;
using System.ComponentModel;
using System.Windows;

using GinRummy.Client.Localization;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Base window of every screen. It keeps the window subscribed to the culture change so
    /// that the texts built from a format string can be rebuilt, releases the subscription
    /// when the window closes, and places every screen over the window it belongs to: the
    /// screens of the menu over the menu, the lobby in place of the menu, and the match over
    /// the lobby.
    /// </summary>
    public class GuiWindowBase : Window
    {
        private readonly LocalizationProvider _localization;
        private bool _returnsToMenu;
        private bool _isHandingOver;
        private bool _isClosed;

        /// <summary>
        /// Subscribes the window to the culture change.
        /// </summary>
        protected GuiWindowBase()
        {
            _localization = LocalizationProvider.Instance;
            _localization.PropertyChanged += OnLocalizationChanged;
            Closed += OnWindowClosed;
        }

        /// <summary>
        /// Gets the localization provider shared by the whole application.
        /// </summary>
        protected LocalizationProvider Localization
        {
            get { return _localization; }
        }

        /// <summary>
        /// Rebuilds the texts that are assembled from a format string. Screens that show
        /// counters, durations or interpolated values override it.
        /// </summary>
        protected virtual void RefreshFormattedText()
        {
        }

        /// <summary>
        /// Opens a screen over this one, which stays visible beneath it, as the screens of the
        /// menu do over the menu and the match does over the lobby.
        /// </summary>
        /// <param name="nextScreen">Screen that opens over this one.</param>
        protected void OpenOver(GuiWindowBase nextScreen)
        {
            nextScreen.Owner = this;
            nextScreen.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            nextScreen.Show();
        }

        /// <summary>
        /// Opens the next screen of the flow over the same window this one belongs to and
        /// closes this one.
        /// </summary>
        /// <param name="nextScreen">Screen that takes the place of this one.</param>
        protected void NavigateTo(GuiWindowBase nextScreen)
        {
            if (nextScreen.Owner == null)
            {
                nextScreen.Owner = Owner;
            }

            _isHandingOver = true;
            nextScreen.Show();
            Close();
        }

        /// <summary>
        /// Opens a lobby in place of the main menu, which hides until the lobby closes. The
        /// screen that led to the lobby closes too, unless it is the menu itself.
        /// </summary>
        /// <param name="lobby">Lobby the player enters.</param>
        protected void EnterLobby(GuiWindowBase lobby)
        {
            Window mainMenu = Application.Current.MainWindow;
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
        /// Leaves this screen for the main menu and opens a screen of the menu over it, as a
        /// guest does when it decides to sign in from its lobby.
        /// </summary>
        /// <param name="nextScreen">Screen that opens over the main menu.</param>
        protected void ReturnToMenuWith(GuiWindowBase nextScreen)
        {
            ShowMainMenu();
            nextScreen.Owner = Application.Current.MainWindow;
            nextScreen.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _isHandingOver = true;
            nextScreen.Show();
            Close();
        }

        /// <summary>
        /// Opens another screen exactly where this one is and closes this one, for the screens
        /// of the prototype that read as two views of the same panel.
        /// </summary>
        /// <param name="nextScreen">Screen that takes the place of this one.</param>
        protected void ReplaceInPlace(GuiWindowBase nextScreen)
        {
            nextScreen.Owner = Owner;
            nextScreen.WindowStartupLocation = WindowStartupLocation.Manual;
            nextScreen.Left = Left;
            nextScreen.Top = Top;
            NavigateTo(nextScreen);
        }

        // The main menu is the main window of the application, so it is found there and not
        // passed from screen to screen along the flow.
        private static void ShowMainMenu()
        {
            GuiWindowBase mainMenu = Application.Current.MainWindow as GuiWindowBase;
            if (mainMenu != null && !mainMenu._isClosed)
            {
                mainMenu.Show();
                mainMenu.Activate();
            }
        }

        private void OnLocalizationChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshFormattedText();
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            _isClosed = true;
            _localization.PropertyChanged -= OnLocalizationChanged;
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
