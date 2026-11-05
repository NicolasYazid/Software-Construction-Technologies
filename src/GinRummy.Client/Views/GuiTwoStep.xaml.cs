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

        // Each purpose reaches its own service, so the screen receives the view model of the purpose it serves.
        // Only the sign-in leaves the screen somewhere else, so only its events are followed apart.
        public GuiTwoStep(TwoStepViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.CodeVerified += OnCodeVerified;
            _viewModel.ErrorDialogRequested += OnErrorDialogRequested;
            _logInViewModel = viewModel as TwoStepLogInViewModel;
            if (_logInViewModel != null)
            {
                _logInViewModel.SignedIn += OnSignedIn;
                _logInViewModel.AccountRestricted += OnAccountRestricted;
            }

            DataContext = _viewModel;
            Closed += OnScreenClosed;
        }

        private void OnCodeVerified(object sender, EventArgs e)
        {
            Close();
        }

        private void OnSignedIn(object sender, LogInOutcome signedInPlayer)
        {
            ((App)Application.Current).StoreSignedInPlayer(signedInPlayer);
            EnterLobby(new GuiLobbyChat());
        }

        private void OnAccountRestricted(object sender, AccountRestriction restriction)
        {
            NavigateTo(AccountRestrictionScreenCommon.CreateScreen(restriction));
        }

        private void OnErrorDialogRequested(object sender, ErrorDialogKind kind)
        {
            ShowModal(new GuiErrorDialog(kind));
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.CodeCountdown.Stop();
            _viewModel.CodeVerified -= OnCodeVerified;
            _viewModel.ErrorDialogRequested -= OnErrorDialogRequested;
            if (_logInViewModel != null)
            {
                _logInViewModel.SignedIn -= OnSignedIn;
                _logInViewModel.AccountRestricted -= OnAccountRestricted;
            }

            Closed -= OnScreenClosed;
        }
    }
}
