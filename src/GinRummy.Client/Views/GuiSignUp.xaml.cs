using System;
using System.Windows;

using GinRummy.Client.Controls;
using GinRummy.Client.Models;
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
            _viewModel.AccountCreated += HandleAccountCreated;
            _viewModel.ConnectionFailed += HandleConnectionFailed;
            DataContext = _viewModel;
            Closed += HandleScreenClosed;
        }

        private void HandleTogglePasswordClick(object sender, RoutedEventArgs e)
        {
            PasswordRevealCommon.SwapVisibleField(pwdPassword, txtPasswordShown);
        }

        private void HandleToggleConfirmPasswordClick(object sender, RoutedEventArgs e)
        {
            PasswordRevealCommon.SwapVisibleField(pwdConfirmPassword, txtConfirmPasswordShown);
        }

        private void HandleCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // The verification screen works with the email as the server stored it (CU-01 step 11).
        private void HandleAccountCreated(object sender, SignUpOutcome outcome)
        {
            SignUpVerifyEmailViewModel verifyEmailViewModel =
                ((App)Application.Current).CreateSignUpVerifyEmailViewModel(outcome.Email);
            if (!outcome.IsVerificationCodeSent)
            {
                verifyEmailViewModel.ShowCodeNotSent();
            }

            NavigateTo(new GuiVerifyEmail(verifyEmailViewModel));
        }

        private void HandleConnectionFailed(object sender, ErrorDialogKind kind)
        {
            ShowModal(new GuiErrorDialog(kind));
        }

        private void HandleScreenClosed(object sender, EventArgs e)
        {
            _viewModel.AccountCreated -= HandleAccountCreated;
            _viewModel.ConnectionFailed -= HandleConnectionFailed;
            Closed -= HandleScreenClosed;
        }
    }
}
