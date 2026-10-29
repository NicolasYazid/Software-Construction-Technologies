using System;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Password recovery screen (P07). Implements the first step of CU-08.
    /// </summary>
    public partial class GuiRecoverPassword : GuiModalBase
    {
        private readonly RecoverPasswordViewModel _viewModel;

        public GuiRecoverPassword()
        {
            InitializeComponent();
            _viewModel = new RecoverPasswordViewModel();
            _viewModel.CodeRequested += OnCodeRequested;
            DataContext = _viewModel;
        }

        private void OnCodeRequested(object sender, EventArgs e)
        {
            NavigateTo(new GuiVerifyEmail(VerificationPurpose.PasswordRecovery, _viewModel.Email));
        }
    }
}
