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

        private void SignIn()
        {
            string password = PasswordRevealCommon.Read(pwdPassword, txtPasswordShown);
            App application = (App)Application.Current;
            LogInController logInController = application.CreateLogInController();
            LogInResult result = logInController.SignIn(txtEmail.Text, password);

            if (result.IsSuccessful)
            {
                ShowSuccess();
            }
            else
            {
                ShowError(result.ErrorMessageKey);
            }
        }

        // The lobby enters in place of the main menu, which stays hidden as the main window of the application.
        // This way, closing the session returns to the menu instead of ending the application (CU-03 step 5).
        // The session, the ban checks and the two-step verification of CU-02 are not applied because they belong to the server.
        private void ShowSuccess()
        {
            EnterLobby(new GuiLobbyChat());
        }

        private void ShowError(string messageKey)
        {
            lblErrorMessage.Text = Localization.GetText(messageKey);
            lblErrorMessage.Visibility = Visibility.Visible;
        }
    }
}
