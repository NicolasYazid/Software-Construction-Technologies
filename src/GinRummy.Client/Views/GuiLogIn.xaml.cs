using System;
using System.Windows;

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
            _viewModel.SignedIn += OnSignedIn;
            _viewModel.TwoFactorRequired += OnTwoFactorRequired;
            _viewModel.AccountRestricted += OnAccountRestricted;
            _viewModel.ConnectionFailed += OnConnectionFailed;
            DataContext = _viewModel;
            Closed += OnScreenClosed;
        }

        private void OnTogglePasswordClick(object sender, RoutedEventArgs e)
        {
            PasswordRevealCommon.Toggle(pwdPassword, txtPasswordShown);
        }

        private void OnForgotPasswordClick(object sender, RoutedEventArgs e)
        {
            NavigateTo(new GuiRecoverPassword());
        }

        private void OnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            NavigateTo(new GuiSignUp());
        }

        private void OnSignedIn(object sender, LogInOutcome signedInPlayer)
        {
            ((App)Application.Current).StoreSignedInPlayer(signedInPlayer);
            EnterLobby(new GuiLobbyChat());
        }

        private void OnTwoFactorRequired(object sender, LogInCredentials credentials)
        {
            TwoStepLogInViewModel twoStepViewModel = ((App)Application.Current).CreateTwoStepLogInViewModel(credentials);
            NavigateTo(new GuiTwoStep(twoStepViewModel));
        }

        private void OnAccountRestricted(object sender, AccountRestriction restriction)
        {
            NavigateTo(AccountRestrictionScreenCommon.CreateScreen(restriction));
        }

        // The refused connection offers to retry, and retrying sends the same sign-in again (CU-02 EX-06).
        private void OnConnectionFailed(object sender, ErrorDialogKind kind)
        {
            GuiErrorDialog errorDialog = new GuiErrorDialog(kind);
            errorDialog.Closed += OnErrorDialogClosed;
            ShowModal(errorDialog);
        }

        private void OnErrorDialogClosed(object sender, EventArgs e)
        {
            GuiErrorDialog errorDialog = (GuiErrorDialog)sender;
            errorDialog.Closed -= OnErrorDialogClosed;
            if (errorDialog.IsActionChosen)
            {
                _viewModel.LogInCommand.Execute(null);
            }
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.SignedIn -= OnSignedIn;
            _viewModel.TwoFactorRequired -= OnTwoFactorRequired;
            _viewModel.AccountRestricted -= OnAccountRestricted;
            _viewModel.ConnectionFailed -= OnConnectionFailed;
            Closed -= OnScreenClosed;
        }
    }
}
