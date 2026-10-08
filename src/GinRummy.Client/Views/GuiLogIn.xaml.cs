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

        // The composition root supplies a ready-made controller, so the screen only runs the use case and reacts to its result.
        private void SignIn()
        {
            string password = PasswordRevealCommon.Read(pwdPassword, txtPasswordShown);
            App application = (App)Application.Current;
            LogInController logInController = application.CreateLogInController();
            LogInResult result = logInController.SignIn(txtEmail.Text, password);

            if (result.Succeeded)
            {
                ShowSuccess();
            }
            else
            {
                ShowError(result.ErrorMessageKey);
            }
        }

        // The lobby takes the place of the main menu, which stays the main window of the application while hidden.
        // Closing the session then brings the player back to the main menu instead of ending the application (CU-03 step 5).
        // The full CU-02 (session, bans and second factor) is still server-dependent.
        private void ShowSuccess()
        {
            // TODO: full CU-02 (session, bans, 2FA, real lobby data) is server-dependent.
            EnterLobby(new GuiLobbyChat());
        }

        private void ShowError(string messageKey)
        {
            lblErrorMessage.Text = Localization.GetText(messageKey);
            lblErrorMessage.Visibility = Visibility.Visible;
        }
    }
}