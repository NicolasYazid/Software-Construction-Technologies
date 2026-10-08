using System.Windows;

using GinRummy.Client.Controllers;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Sign-up screen (P02). Implements CU-01: reads what the user typed, asks the
    /// controller to create the account, and reacts to the result.
    /// </summary>
    public partial class GuiSignUp : GuiModalBase
    {
        private const string PasswordMismatchKey = "Error_ValPasswordMismatch";
        private const string DemoCodeMessageKey = "SignUp_DemoCodeGenerated";

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
            // Every other validation runs inside the controller.
            string password = PasswordRevealCommon.Read(pwdPassword, txtPasswordShown);
            string confirmation = PasswordRevealCommon.Read(pwdConfirmPassword, txtConfirmPasswordShown);
            bool isPasswordConfirmed = password == confirmation;
            if (!isPasswordConfirmed)
            {
                ShowError(PasswordMismatchKey);
            }
            else
            {
                CreateAccount(password);
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // The composition root supplies a ready-made controller, so the view never builds its dependencies.
        private void CreateAccount(string password)
        {
            App application = (App)Application.Current;
            SignUpController signUpController = application.CreateSignUpController();
            SignUpResult result = signUpController.CreateAccount(
                txtEmail.Text,
                txtUsername.Text,
                password,
                Localization.CurrentCulture.Name);

            if (result.Succeeded)
            {
                ShowGeneratedCodeAndContinue(result.GeneratedCode);
            }
            else
            {
                ShowError(result);
            }
        }

        // The generated code is shown on screen in place of the email delivery, which is out of scope without a server.
        private void ShowGeneratedCodeAndContinue(string generatedCode)
        {
            lblErrorMessage.Visibility = Visibility.Collapsed;
            MessageBox.Show(Localization.Format(DemoCodeMessageKey, generatedCode));

            // TODO: When the server exists, the code will be delivered by email.
            // The verification screen will then check the typed code against the stored hash and mark the account verified.
            // That loop is server-dependent and will be completed later.
            NavigateTo(new GuiVerifyEmail(VerificationPurpose.AccountSignUp, txtEmail.Text));
        }

        private void ShowError(string messageKey)
        {
            ShowErrorText(Localization.GetText(messageKey));
        }

        // A message with a placeholder, such as the maximum length of a field, is filled with the active culture.
        // That way the placeholder is never shown to the player.
        private void ShowError(SignUpResult result)
        {
            string message = Localization.GetText(result.ErrorMessageKey);
            if (result.ErrorMessageArgument.HasValue)
            {
                message = Localization.Format(result.ErrorMessageKey, result.ErrorMessageArgument.Value);
            }

            ShowErrorText(message);
        }

        private void ShowErrorText(string message)
        {
            lblErrorMessage.Text = message;
            lblErrorMessage.Visibility = Visibility.Visible;
        }
    }
}
