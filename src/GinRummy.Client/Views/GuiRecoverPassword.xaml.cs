using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Password recovery screen (P07). Implements the first step of CU-08.
    /// </summary>
    public partial class GuiRecoverPassword : GuiModalBase
    {
        /// <summary>
        /// Builds the password recovery screen.
        /// </summary>
        public GuiRecoverPassword()
        {
            InitializeComponent();
        }

        private void OnSendCodeClick(object sender, RoutedEventArgs e)
        {
            // CU-08 answers the same way whether or not the address has an account, so that
            // the screen never reveals which addresses are registered.
            lblCodeSentIfExists.Visibility = Visibility.Visible;
            NavigateTo(new GuiVerifyEmail(VerificationPurpose.PasswordRecovery, txtEmail.Text));
        }
    }
}
