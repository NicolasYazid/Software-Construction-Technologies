using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Main menu screen (P01). Entry point of the client and the place where the player
    /// chooses the language of the interface.
    /// </summary>
    public partial class GuiMainMenu : GuiWindowBase
    {
        /// <summary>
        /// Builds the main menu.
        /// </summary>
        public GuiMainMenu()
        {
            InitializeComponent();
        }

        private void OnLogInClick(object sender, RoutedEventArgs e)
        {
            GuiLogIn logIn = new GuiLogIn();
            logIn.Owner = this;
            logIn.Show();
        }

        private void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            GuiSignUp signUp = new GuiSignUp();
            signUp.Owner = this;
            signUp.Show();
        }

        private void OnPlayAsGuestClick(object sender, RoutedEventArgs e)
        {
            // The guest lobby (P16) belongs to a later delivery. The button stays wired so
            // that the navigation of the prototype is complete.
        }

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            // The house rules screen (P19) belongs to a later delivery.
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
