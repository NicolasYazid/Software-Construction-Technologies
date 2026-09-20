using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Sign-up screen (P02). Implements CU-01.
    /// </summary>
    public partial class GuiSignUp : GuiWindowBase
    {
        /// <summary>
        /// Builds the sign-up screen.
        /// </summary>
        public GuiSignUp()
        {
            InitializeComponent();
        }

        private void OnTogglePasswordClick(object sender, RoutedEventArgs e)
        {
            // Showing the password in clear text needs a dedicated control, which belongs to
            // the delivery that builds the reusable fields.
        }

        private void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            // CU-01 FA-04 is the only check the specification allows on the client, because
            // it sends nothing to the server. Every other validation runs on the server, as
            // CON-07 requires.
            bool passwordsMatch = pwdPassword.Password == pwdConfirmPassword.Password;
            if (passwordsMatch)
            {
                lblErrorMessage.Visibility = Visibility.Collapsed;
                GuiVerifyEmail verifyEmail = new GuiVerifyEmail(
                    VerificationPurpose.AccountSignUp, txtEmail.Text);
                verifyEmail.Owner = Owner;
                verifyEmail.Show();
                Close();
            }
            else
            {
                lblErrorMessage.Text = Localization.GetText("Error_ValPasswordMismatch");
                lblErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
