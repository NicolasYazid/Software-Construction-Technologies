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
            _viewModel.CodeVerified += HandleCodeVerified;
            _viewModel.ErrorDialogRequested += HandleErrorDialogRequested;
            _logInViewModel = viewModel as TwoStepLogInViewModel;
            if (_logInViewModel != null)
            {
                _logInViewModel.SignedIn += HandleSignedIn;
                _logInViewModel.AccountRestricted += HandleAccountRestricted;
            }

            DataContext = _viewModel;
            Closed += HandleScreenClosed;
        }

        private void HandleCodeVerified(object sender, EventArgs e)
        {
            Close();
        }

        private void HandleSignedIn(object sender, LogInOutcome signedInPlayer)
        {
            ((App)Application.Current).StoreSignedInPlayer(signedInPlayer);
            EnterLobby(new GuiLobbyChat());
        }

        private void HandleAccountRestricted(object sender, AccountRestriction restriction)
        {
            NavigateTo(AccountRestrictionScreenCommon.CreateScreen(restriction));
        }

        private void HandleErrorDialogRequested(object sender, ErrorDialogKind kind)
        {
            ShowModal(new GuiErrorDialog(kind));
        }

        private void HandleScreenClosed(object sender, EventArgs e)
        {
            _viewModel.CodeCountdown.Stop();
            _viewModel.CodeVerified -= HandleCodeVerified;
            _viewModel.ErrorDialogRequested -= HandleErrorDialogRequested;
            if (_logInViewModel != null)
            {
                _logInViewModel.SignedIn -= HandleSignedIn;
                _logInViewModel.AccountRestricted -= HandleAccountRestricted;
            }

            Closed -= HandleScreenClosed;
        }
    }
}
