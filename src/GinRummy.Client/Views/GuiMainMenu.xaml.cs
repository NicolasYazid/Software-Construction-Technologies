using System.Windows;

using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Main menu screen (P01). Entry point of the client and the place where the player
    /// chooses the language of the interface.
    /// </summary>
    public partial class GuiMainMenu : GuiWindowBase
    {
        public GuiMainMenu()
        {
            InitializeComponent();
        }

        private void HandleLogInClick(object sender, RoutedEventArgs e)
        {
            ShowModal(new GuiLogIn());
        }

        private void HandleCreateAccountClick(object sender, RoutedEventArgs e)
        {
            ShowModal(new GuiSignUp());
        }

        private void HandlePlayAsGuestClick(object sender, RoutedEventArgs e)
        {
            LogInViewModel logIn = ((App)Application.Current).CreateLogInViewModel();
            logIn.GuestSignedIn += HandleGuestSignedIn;
            logIn.ConnectionFailed += HandleConnectionFailed;
            logIn.LogInAsGuest();
        }

        private void HandleGuestSignedIn(object sender, LogInOutcome signedInGuest)
        {
            ((App)Application.Current).StoreSignedInGuest(signedInGuest);
            EnterLobby(new GuiGuestLobby());
        }

        private void HandleConnectionFailed(object sender, ErrorDialogKind kind)
        {
            ShowModal(new GuiErrorDialog(kind));
        }

        private void HandleSettingsClick(object sender, RoutedEventArgs e)
        {
            ShowModal(new GuiProfilePanel());
        }

        private void HandleHowToPlayClick(object sender, RoutedEventArgs e)
        {
            GuiHouseRules houseRules = new GuiHouseRules();
            ShowModal(houseRules);
        }

        private void HandleExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
