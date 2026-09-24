using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Sign-in screen (P05). Implements CU-02.
    /// </summary>
    public partial class GuiLogIn : GuiWindowBase
    {
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
            // Credentials travel to the server, which is the only component that validates
            // them. The screen only walks the navigation of the prototype.
            GuiTwoStep twoStep = new GuiTwoStep(TwoStepPurpose.LogIn);
            twoStep.Owner = Owner;
            twoStep.Show();
            Close();
        }

        private void OnForgotPasswordClick(object sender, RoutedEventArgs e)
        {
            GuiRecoverPassword recoverPassword = new GuiRecoverPassword();
            recoverPassword.Owner = Owner;
            recoverPassword.Show();
            Close();
        }

        private void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            GuiSignUp signUp = new GuiSignUp();
            signUp.Owner = Owner;
            signUp.Show();
            Close();
        }
    }
}
