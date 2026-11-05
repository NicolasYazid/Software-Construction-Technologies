using System;
using System.Windows;

using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Verification code screen (P03). Implements CU-09 and the verification step of CU-01,
    /// CU-07 and CU-08. Every duration and counter it shows is formatted with the active
    /// culture and never written inside the string.
    /// </summary>
    public partial class GuiVerifyEmail : GuiModalBase
    {
        private readonly VerifyEmailViewModel _viewModel;

        public GuiVerifyEmail()
            : this(VerificationPurpose.AccountSignUp, string.Empty)
        {
        }

        public GuiVerifyEmail(VerificationPurpose purpose, string destinationAddress)
            : this(((App)Application.Current).CreateVerifyEmailViewModel(purpose, destinationAddress))
        {
        }

        // The sign-up and the change of email have their own view models because they reach the server.
        // The recovery of the password still uses the prototype.
        public GuiVerifyEmail(VerifyEmailViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.CodeVerified += OnCodeVerified;
            _viewModel.ErrorDialogRequested += OnErrorDialogRequested;

            DataContext = _viewModel;
            Closed += OnScreenClosed;
        }

        private void OnCodeVerified(object sender, EventArgs e)
        {
            if (_viewModel.Purpose == VerificationPurpose.PasswordRecovery)
            {
                NavigateTo(new GuiNewPassword());
            }
            else if (_viewModel.Purpose == VerificationPurpose.AccountSignUp)
            {
                NavigateTo(new GuiAccountCreated());
            }
            else
            {
                Close();
            }
        }

        private void OnErrorDialogRequested(object sender, ErrorDialogKind kind)
        {
            ShowModal(new GuiErrorDialog(kind));
        }

        private void OnLogInClick(object sender, RoutedEventArgs e)
        {
            NavigateTo(new GuiLogIn());
        }

        private void OnVerifyLaterClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.StopCountdowns();
            _viewModel.CodeVerified -= OnCodeVerified;
            _viewModel.ErrorDialogRequested -= OnErrorDialogRequested;

            Closed -= OnScreenClosed;
        }
    }
}
