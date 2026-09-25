using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Sign-up screen (P02). Implements CU-01.
    /// </summary>
    public partial class GuiSignUp : GuiModalBase
    {
        private const string PasswordMismatchKey = "Error_ValPasswordMismatch";

        /// <summary>
        /// Builds the sign-up screen.
        /// </summary>
        public GuiSignUp()
        {
            InitializeComponent();
        }

        private void OnTogglePasswordClick(object sender, RoutedEventArgs e)
        {
            PasswordRevealCommon.Toggle(pwdPassword, txtPasswordShown);
        }

        private void OnToggleConfirmPasswordClick(object sender, RoutedEventArgs e)
        {
            PasswordRevealCommon.Toggle(pwdConfirmPassword, txtConfirmPasswordShown);
        }

        private void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            // CU-01 FA-04 is the only check the specification allows on the client, because
            // it sends nothing to the server. Every other validation runs on the server, as
            // CON-07 requires.
            string password = PasswordRevealCommon.Read(pwdPassword, txtPasswordShown);
            string confirmation = PasswordRevealCommon.Read(pwdConfirmPassword, txtConfirmPasswordShown);
            bool passwordsMatch = password == confirmation;
            if (passwordsMatch)
            {
                lblErrorMessage.Visibility = Visibility.Collapsed;
                NavigateTo(new GuiVerifyEmail(VerificationPurpose.AccountSignUp, txtEmail.Text));
            }
            else
            {
                lblErrorMessage.Text = Localization.GetText(PasswordMismatchKey);
                lblErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
