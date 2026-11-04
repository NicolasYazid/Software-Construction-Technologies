using System;
using System.Windows;

using GinRummy.Client.SignUpServiceReference;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Sign-up screen (P02). Implements CU-01: reads what the user typed, asks the sign-up service to create
    /// the account, and goes to the verification of the email when it was created.
    /// </summary>
    public partial class GuiSignUp : GuiModalBase
    {
        private readonly SignUpViewModel _viewModel;

        public GuiSignUp()
            : this(false)
        {
        }

        public GuiSignUp(bool isGuestSessionActive)
        {
            InitializeComponent();
            _viewModel = ((App)Application.Current).CreateSignUpViewModel(isGuestSessionActive);
            _viewModel.AccountCreated += OnAccountCreated;
            _viewModel.ConnectionFailed += OnConnectionFailed;
            DataContext = _viewModel;
            Closed += OnScreenClosed;
        }

        private void OnTogglePasswordClick(object sender, RoutedEventArgs e)
        {
            PasswordRevealCommon.Toggle(pwdPassword, txtPasswordShown);
        }

        private void OnToggleConfirmPasswordClick(object sender, RoutedEventArgs e)
        {
            PasswordRevealCommon.Toggle(pwdConfirmPassword, txtConfirmPasswordShown);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // The verification screen works with the email as the server stored it (CU-01 step 11).
        private void OnAccountCreated(object sender, SignUpOutcome outcome)
        {
            SignUpVerifyEmailViewModel verifyEmailViewModel =
                ((App)Application.Current).CreateSignUpVerifyEmailViewModel(outcome.Email);
            if (!outcome.IsVerificationCodeSent)
            {
                verifyEmailViewModel.ShowCodeNotSent();
            }

            NavigateTo(new GuiVerifyEmail(verifyEmailViewModel));
        }

        private void OnConnectionFailed(object sender, ErrorDialogKind kind)
        {
            ShowModal(new GuiErrorDialog(kind));
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.AccountCreated -= OnAccountCreated;
            _viewModel.ConnectionFailed -= OnConnectionFailed;
            Closed -= OnScreenClosed;
        }
    }
}
