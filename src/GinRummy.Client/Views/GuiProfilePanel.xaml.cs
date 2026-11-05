using System;
using System.Windows;

using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Profile panel (P14). Holds the settings of the account: the change of email of CU-07
    /// and of password of CU-06, the second factor of CU-04 and CU-05, the language of CU-20
    /// and the closing of the session of CU-03.
    /// </summary>
    public partial class GuiProfilePanel : GuiModalBase
    {
        private const int PasswordMaskLength = 10;
        private const char PasswordMaskFiller = 'x';

        private readonly ProfilePanelViewModel _viewModel;

        public GuiProfilePanel()
        {
            InitializeComponent();
            _viewModel = ((App)Application.Current).CreateProfilePanelViewModel();
            _viewModel.EmailVerificationRequested += OnEmailVerificationRequested;
            _viewModel.TwoStepEnableRequested += OnTwoStepEnableRequested;
            _viewModel.TwoStepDisableRequested += OnTwoStepDisableRequested;
            DataContext = _viewModel;
            pwdPassword.Password = new string(PasswordMaskFiller, PasswordMaskLength);
            txtNewEmail.IsVisibleChanged += OnNewEmailVisibleChanged;
        }

        private void OnNewEmailVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (txtNewEmail.IsVisible)
            {
                txtNewEmail.Focus();
            }
        }

        private void OnEmailVerificationRequested(object sender, EventArgs e)
        {
            GuiVerifyEmail verifyEmail = new GuiVerifyEmail(VerificationPurpose.EmailChange, _viewModel.PendingEmail);
            verifyEmail.Closed += OnVerifyEmailClosed;
            ShowModal(verifyEmail);
        }

        private void OnVerifyEmailClosed(object sender, EventArgs e)
        {
            _viewModel.CompleteEmailChange();
        }

        private void OnChangePasswordClick(object sender, RoutedEventArgs e)
        {
            GuiNewPassword newPassword = new GuiNewPassword(true);
            ShowModal(newPassword);
        }

        // The verification screen is the one that talks to the server, so it confirms the code that enables the second factor.
        private void OnTwoStepEnableRequested(object sender, EventArgs e)
        {
            GuiTwoStep twoStep = new GuiTwoStep(TwoStepPurpose.EnableTwoStep);
            ShowModal(twoStep);
        }

        private void OnTwoStepDisableRequested(object sender, EventArgs e)
        {
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.TwoStepDisable);
            confirmDialog.Closed += OnTwoStepDisableConfirmClosed;
            ShowModal(confirmDialog);
        }

        private void OnTwoStepDisableConfirmClosed(object sender, EventArgs e)
        {
            _viewModel.CompleteTwoStepDisable(((GuiConfirmDialog)sender).IsConfirmed);
        }

        // Closing the lobby ends the presence subscription, so the other players see the player leave.
        // The token is forgotten too, so nobody else can use the account on this computer (CU-03).
        private void OnLogOutClick(object sender, RoutedEventArgs e)
        {
            GuiWindowBase lobby = Host;
            Close();
            if (lobby != null)
            {
                lobby.Close();
            }

            ((App)Application.Current).ForgetSignedInPlayer();
        }
    }
}
