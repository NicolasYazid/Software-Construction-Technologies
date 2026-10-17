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
            // TODO: pendiente migrar a WCF. The sign-in stays inactive until the server exposes the CU-02 service.
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
