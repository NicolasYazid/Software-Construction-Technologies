using System;
using System.Windows;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// New password screen (P08). Serves CU-06 and the last step of CU-08. Both the title and
    /// the current password field depend on the flow the screen was opened from.
    /// </summary>
    public partial class GuiNewPassword : GuiModalBase
    {
        private static readonly Thickness _newPasswordLabelAfterText = new Thickness(0, 28, 0, 8);
        private static readonly Thickness _newPasswordLabelAfterField = new Thickness(0, 22, 0, 8);

        private readonly NewPasswordViewModel _viewModel;

        public GuiNewPassword()
            : this(false)
        {
        }

        public GuiNewPassword(bool isChangeFromProfile)
        {
            InitializeComponent();
            _viewModel = new NewPasswordViewModel(isChangeFromProfile);
            _viewModel.PasswordUpdated += OnPasswordUpdated;
            DataContext = _viewModel;
            ApplyFlowLayout();
        }

        // The label of the new password sits closer to a field above it than to the instructions.
        private void ApplyFlowLayout()
        {
            Thickness newPasswordLabelMargin = _newPasswordLabelAfterText;
            if (_viewModel.IsChangeFromProfile)
            {
                newPasswordLabelMargin = _newPasswordLabelAfterField;
            }

            lblNewPassword.Margin = newPasswordLabelMargin;
        }

        // A recovered password ends every session of the account, so the player signs in again with it (CU-08 step 14).
        private void OnPasswordUpdated(object sender, EventArgs e)
        {
            if (_viewModel.IsChangeFromProfile)
            {
                Close();
            }
            else
            {
                NavigateTo(new GuiLogIn());
            }
        }
    }
}
