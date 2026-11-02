using System;
using System.Windows;

using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Two-step verification screen (P06). Serves CU-02 FA-04, CU-04 and CU-06.
    /// </summary>
    public partial class GuiTwoStep : GuiModalBase
    {
        private readonly TwoStepViewModel _viewModel;
        private readonly TwoStepLogInViewModel _logInViewModel;

        public GuiTwoStep(TwoStepPurpose purpose)
            : this(new TwoStepViewModel(purpose))
        {
        }

        // The sign-in has its own view model because only it already reaches the server; the other purposes still use the prototype.
        public GuiTwoStep(TwoStepViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.CodeVerified += OnCodeVerified;
            _logInViewModel = viewModel as TwoStepLogInViewModel;
            if (_logInViewModel != null)
            {
                _logInViewModel.SignedIn += OnSignedIn;
                _logInViewModel.AccountRestricted += OnAccountRestricted;
                _logInViewModel.ConnectionFailed += OnConnectionFailed;
            }

            DataContext = _viewModel;
            Closed += OnScreenClosed;
        }

        private void OnCodeVerified(object sender, EventArgs e)
        {
            Close();
        }

        // A sign-in ends in the lobby (CU-02 step 11).
        private void OnSignedIn(object sender, LogInOutcome signedInPlayer)
        {
            ((App)Application.Current).StoreSignedInPlayer(signedInPlayer);
            EnterLobby(new GuiLobbyChat());
        }

        private void OnAccountRestricted(object sender, AccountRestriction restriction)
        {
            NavigateTo(AccountRestrictionScreenCommon.CreateScreen(restriction));
        }

        private void OnConnectionFailed(object sender, ErrorDialogKind kind)
        {
            ShowModal(new GuiErrorDialog(kind));
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.CodeCountdown.Stop();
            _viewModel.CodeVerified -= OnCodeVerified;
            if (_logInViewModel != null)
            {
                _logInViewModel.SignedIn -= OnSignedIn;
                _logInViewModel.AccountRestricted -= OnAccountRestricted;
                _logInViewModel.ConnectionFailed -= OnConnectionFailed;
            }

            Closed -= OnScreenClosed;
        }
    }
}
