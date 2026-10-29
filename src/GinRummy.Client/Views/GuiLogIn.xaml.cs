using System.Windows;

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
            // The sign-in waits for the CU-02 service, which the server does not expose yet.
        }

        private void OnForgotPasswordClick(object sender, RoutedEventArgs e)
        {
            NavigateTo(new GuiRecoverPassword());
        }

        private void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            NavigateTo(new GuiSignUp());
        }
    }
}
