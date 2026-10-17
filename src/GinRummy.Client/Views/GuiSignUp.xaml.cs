using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Sign-up screen (P02). Implements CU-01: reads what the user typed, asks the
    /// controller to create the account, and reacts to the result.
    /// </summary>
    public partial class GuiSignUp : GuiModalBase
    {
        private const string PasswordMismatchKey = "Error_ValPasswordMismatch";

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
            // Matching the password with its confirmation is the only check the client may resolve on its own (CU-01 FA-04).
            // Every other validation belongs to the server.
            string password = PasswordRevealCommon.Read(pwdPassword, txtPasswordShown);
            string confirmation = PasswordRevealCommon.Read(pwdConfirmPassword, txtConfirmPasswordShown);
            bool isPasswordConfirmed = password == confirmation;
            if (!isPasswordConfirmed)
            {
                ShowError(PasswordMismatchKey);
            }
            else
            {
                // TODO: pendiente migrar a WCF. The account creation stays inactive until the server exposes the CU-01 service.
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowError(string messageKey)
        {
            ShowErrorText(Localization.GetText(messageKey));
        }

        private void ShowErrorText(string message)
        {
            lblErrorMessage.Text = message;
            lblErrorMessage.Visibility = Visibility.Visible;
        }
    }
}
