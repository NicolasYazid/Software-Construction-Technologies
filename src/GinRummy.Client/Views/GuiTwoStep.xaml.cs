using System;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Two-step verification screen (P06). Serves CU-02 FA-04, CU-04 and CU-06.
    /// </summary>
    public partial class GuiTwoStep : GuiModalBase
    {
        private readonly TwoStepViewModel _viewModel;

        public GuiTwoStep()
            : this(TwoStepPurpose.LogIn)
        {
        }

        public GuiTwoStep(TwoStepPurpose purpose)
        {
            InitializeComponent();
            _viewModel = new TwoStepViewModel(purpose);
            _viewModel.CodeVerified += OnCodeVerified;
            DataContext = _viewModel;
            Closed += OnScreenClosed;
        }

        // A sign-in ends in the lobby (CU-02 step 11), while the other flows return to the screen that asked for the code.
        private void OnCodeVerified(object sender, EventArgs e)
        {
            if (_viewModel.Purpose == TwoStepPurpose.LogIn)
            {
                EnterLobby(new GuiLobbyChat());
            }
            else
            {
                Close();
            }
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.CodeCountdown.Stop();
            _viewModel.CodeVerified -= OnCodeVerified;
            Closed -= OnScreenClosed;
        }
    }
}
