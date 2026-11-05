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

        private void OnLogInClick(object sender, RoutedEventArgs e)
        {
            ShowModal(new GuiLogIn());
        }

        private void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            ShowModal(new GuiSignUp());
        }

        private void OnPlayAsGuestClick(object sender, RoutedEventArgs e)
        {
            LogInViewModel logIn = ((App)Application.Current).CreateLogInViewModel();
            logIn.GuestSignedIn += OnGuestSignedIn;
            logIn.ConnectionFailed += OnConnectionFailed;
            logIn.LogInAsGuest();
        }

        private void OnGuestSignedIn(object sender, LogInOutcome signedInGuest)
        {
            ((App)Application.Current).StoreSignedInGuest(signedInGuest);
            EnterLobby(new GuiGuestLobby());
        }

        private void OnConnectionFailed(object sender, ErrorDialogKind kind)
        {
            ShowModal(new GuiErrorDialog(kind));
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            ShowModal(new GuiProfilePanel());
        }

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            GuiHouseRules houseRules = new GuiHouseRules();
            ShowModal(houseRules);
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
