using System.Windows;

using GinRummy.Client.Controllers;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Sign-in screen (P05). Implements CU-02 at this activity's scope: reads the email and
    /// password, asks the controller to authenticate, and reacts to the result.
    /// </summary>
    public partial class GuiLogIn : GuiModalBase
    {
        private const string SignInSuccessMessageKey = "LogIn_DemoSignInSuccess";

        /// <summary>
        /// Builds the sign-in screen.
        /// </summary>
        public GuiLogIn()
        {
            InitializeComponent();
        }

        private void OnTogglePasswordClick(object sender, RoutedEventArgs e)
        {
            PasswordRevealCommon.Toggle(pwdPassword, txtPasswordShown);
        }

        private void OnLogInClick(object sender, RoutedEventArgs e)
        {
            SignIn();
        }

        private void OnForgotPasswordClick(object sender, RoutedEventArgs e)
        {
            NavigateTo(new GuiRecoverPassword());
        }

        private void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            NavigateTo(new GuiSignUp());
        }

        // Asks the composition root for a ready-made controller, runs the use case, and
        // reacts to its result.
        private void SignIn()
        {
            string password = PasswordRevealCommon.Read(pwdPassword, txtPasswordShown);
            App application = (App)Application.Current;
            LogInController logInController = application.CreateLogInController();
            LogInResult result = logInController.SignIn(txtEmail.Text, password);

            if (result.Succeeded)
            {
                ShowSuccess(result.Username);
            }
            else
            {
                ShowError(result.ErrorMessageKey);
            }
        }

        // On success, the prototype navigates to the lobby, from which the leaderboard is
        // reached. The full CU-02 (creating a PlayerSession, checking bans and 2FA) is
        // server-dependent and completed in a later iteration.
        private void ShowSuccess(string username)
        {
            // TODO: full CU-02 (session, bans, 2FA, real lobby data) is server-dependent.
            GuiLobbyChat lobby = new GuiLobbyChat();
            lobby.Show();
            Application.Current.MainWindow = lobby;
            Close();
        }

        // Shows a localized error message on the card.
        private void ShowError(string messageKey)
        {
            lblErrorMessage.Text = Localization.GetText(messageKey);
            lblErrorMessage.Visibility = Visibility.Visible;
        }
    }
}