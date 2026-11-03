using System;
using System.Windows;

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
        private readonly SignUpVerifyEmailViewModel _signUpViewModel;

        public GuiVerifyEmail()
            : this(VerificationPurpose.AccountSignUp, string.Empty)
        {
        }

        public GuiVerifyEmail(VerificationPurpose purpose, string destinationAddress)
            : this(new VerifyEmailViewModel(purpose, destinationAddress))
        {
        }

        // The sign-up has its own view model because only it already reaches the server; the other purposes still use the prototype.
        public GuiVerifyEmail(VerifyEmailViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.CodeVerified += OnCodeVerified;
            _signUpViewModel = viewModel as SignUpVerifyEmailViewModel;
            if (_signUpViewModel != null)
            {
                _signUpViewModel.ConnectionFailed += OnConnectionFailed;
            }

            DataContext = _viewModel;
            Closed += OnScreenClosed;
        }

        // A new address confirmed from the profile panel returns to that panel (CU-07 step 11).
        private void OnCodeVerified(object sender, EventArgs e)
        {
            if (_viewModel.Purpose == VerificationPurpose.PasswordRecovery)
            {
                NavigateTo(new GuiNewPassword(false));
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

        private void OnConnectionFailed(object sender, ErrorDialogKind kind)
        {
            ShowModal(new GuiErrorDialog(kind));
        }

        // An email that was already verified only needs to sign in (CU-09 FA-02).
        private void OnLogInClick(object sender, RoutedEventArgs e)
        {
            NavigateTo(new GuiLogIn());
        }

        // The pending code and the unverified account are kept, so the verification can be resumed later (CU-09 FA-03).
        private void OnVerifyLaterClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.StopCountdowns();
            _viewModel.CodeVerified -= OnCodeVerified;
            if (_signUpViewModel != null)
            {
                _signUpViewModel.ConnectionFailed -= OnConnectionFailed;
            }

            Closed -= OnScreenClosed;
        }
    }
}
