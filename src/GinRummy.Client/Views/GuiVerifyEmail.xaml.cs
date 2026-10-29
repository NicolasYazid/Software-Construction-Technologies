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

        public GuiVerifyEmail()
            : this(VerificationPurpose.AccountSignUp, string.Empty)
        {
        }

        public GuiVerifyEmail(VerificationPurpose purpose, string destinationAddress)
        {
            InitializeComponent();
            _viewModel = new VerifyEmailViewModel(purpose, destinationAddress);
            _viewModel.CodeVerified += OnCodeVerified;
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

        private void OnVerifyLaterClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.StopCountdowns();
            _viewModel.CodeVerified -= OnCodeVerified;
            Closed -= OnScreenClosed;
        }
    }
}
