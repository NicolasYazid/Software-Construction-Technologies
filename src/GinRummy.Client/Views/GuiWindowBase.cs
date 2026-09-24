using System;
using System.ComponentModel;
using System.Windows;

using GinRummy.Client.Localization;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Base window of every screen. It keeps the window subscribed to the culture change so
    /// that the texts built from a format string can be rebuilt, releases the subscription
    /// when the window closes, and walks the screens of a flow as a single window that takes
    /// the place of the main menu.
    /// </summary>
    public class GuiWindowBase : Window
    {
        private readonly LocalizationProvider _localization;
        private bool _isMenuFlowScreen;
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
        /// Opens a screen of the main menu in its place. The menu hides, so the screen is the
        /// only window of the client until the flow it starts comes to an end.
        /// </summary>
        /// <param name="nextScreen">Screen that takes the place of the main menu.</param>
        protected void OpenInPlaceOfMenu(GuiWindowBase nextScreen)
        {
            nextScreen._isMenuFlowScreen = true;
            nextScreen.Show();
            Hide();
        }

        /// <summary>
        /// Opens the next screen of the flow and closes this one, so the flow keeps a single
        /// window and the main menu stays hidden while it lasts.
        /// </summary>
        /// <param name="nextScreen">Screen that takes the place of this one.</param>
        protected void NavigateTo(GuiWindowBase nextScreen)
        {
            nextScreen._isMenuFlowScreen = _isMenuFlowScreen;
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

            // A flow that ends without handing over to another screen, because the player
            // closed its window or left the lobby, returns to the menu that started it.
            if (_isMenuFlowScreen && !_isHandingOver)
            {
                ShowMainMenu();
            }
        }
    }
}
