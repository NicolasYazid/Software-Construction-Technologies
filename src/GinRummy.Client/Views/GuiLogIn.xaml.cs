using System;
using System.Windows;

using GinRummy.Client.Controls;
using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Sign-in screen (P05). Implements CU-02: reads the email, the password and the keep signed in choice,
    /// asks the sign-in service to authenticate, and goes to the screen the answer calls for.
    /// </summary>
    public partial class GuiLogIn : GuiModalBase
    {
        private readonly LogInViewModel _viewModel;

        public GuiLogIn()
        {
            InitializeComponent();
            _viewModel = ((App)Application.Current).CreateLogInViewModel();
            _viewModel.SignedIn += HandleSignedIn;
            _viewModel.TwoFactorRequired += HandleTwoFactorRequired;
            _viewModel.AccountRestricted += HandleAccountRestricted;
            _viewModel.ConnectionFailed += HandleConnectionFailed;
            DataContext = _viewModel;
            Closed += HandleScreenClosed;
        }

        private void HandleTogglePasswordClick(object sender, RoutedEventArgs e)
        {
            PasswordRevealCommon.SwapVisibleField(pwdPassword, txtPasswordShown);
        }

        private void HandleForgotPasswordClick(object sender, RoutedEventArgs e)
        {
            NavigateTo(new GuiRecoverPassword());
        }

        private void HandleCreateAccountClick(object sender, RoutedEventArgs e)
        {
            NavigateTo(new GuiSignUp());
        }

        // The account was created earlier, so its code may have expired; a new one is asked for once the screen is open (CU-02 EX-03).
        private void HandleResendCodeClick(object sender, RoutedEventArgs e)
        {
            SignUpVerifyEmailViewModel verifyEmailViewModel =
                ((App)Application.Current).CreateSignUpVerifyEmailViewModel(_viewModel.Email);
            NavigateTo(new GuiVerifyEmail(verifyEmailViewModel));
            verifyEmailViewModel.ResendCodeCommand.Execute(null);
        }

        private void HandleSignedIn(object sender, LogInOutcome signedInPlayer)
        {
            ((App)Application.Current).StoreSignedInPlayer(signedInPlayer);
            EnterLobby(new GuiLobbyChat());
        }

        private void HandleTwoFactorRequired(object sender, LogInCredentials credentials)
        {
            TwoStepLogInViewModel twoStepViewModel = ((App)Application.Current).CreateTwoStepLogInViewModel(credentials);
            NavigateTo(new GuiTwoStep(twoStepViewModel));
        }

        private void HandleAccountRestricted(object sender, AccountRestriction restriction)
        {
            NavigateTo(AccountRestrictionScreenCommon.CreateScreen(restriction));
        }

        private void HandleConnectionFailed(object sender, ErrorDialogKind kind)
        {
            GuiErrorDialog errorDialog = new GuiErrorDialog(kind);
            errorDialog.Closed += HandleErrorDialogClosed;
            ShowModal(errorDialog);
        }

        private void HandleErrorDialogClosed(object sender, EventArgs e)
        {
            GuiErrorDialog errorDialog = (GuiErrorDialog)sender;
            errorDialog.Closed -= HandleErrorDialogClosed;
            if (errorDialog.IsActionChosen)
            {
                _viewModel.LogInCommand.Execute(null);
            }
        }

        private void HandleScreenClosed(object sender, EventArgs e)
        {
            _viewModel.SignedIn -= HandleSignedIn;
            _viewModel.TwoFactorRequired -= HandleTwoFactorRequired;
            _viewModel.AccountRestricted -= HandleAccountRestricted;
            _viewModel.ConnectionFailed -= HandleConnectionFailed;
            Closed -= HandleScreenClosed;
        }
    }
}
