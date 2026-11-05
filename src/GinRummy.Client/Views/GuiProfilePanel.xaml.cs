using System;
using System.Windows;
using System.Windows.Controls;

using GinRummy.Client.Localization;
using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Profile panel (P14). Holds the settings of the account: the change of email of CU-07
    /// and of password of CU-06, the second factor of CU-04 and CU-05, the language of CU-20
    /// and the closing of the session of CU-03. Without a session it only holds the language.
    /// </summary>
    public partial class GuiProfilePanel : GuiModalBase
    {
        private const int PasswordMaskLength = 10;
        private const char PasswordMaskFiller = 'x';

        private readonly ProfilePanelViewModel _viewModel;

        public GuiProfilePanel()
            : this(null)
        {
        }

        // The main menu has no session and therefore no profile, so the panel keeps the default avatar there.
        public GuiProfilePanel(PlayerProfile ownProfile)
        {
            InitializeComponent();
            _viewModel = ((App)Application.Current).CreateProfilePanelViewModel();
            _viewModel.EmailVerificationRequested += OnEmailVerificationRequested;
            _viewModel.TwoStepEnableRequested += OnTwoStepEnableRequested;
            _viewModel.TwoStepDisableRequested += OnTwoStepDisableRequested;
            _viewModel.ErrorDialogRequested += OnErrorDialogRequested;
            _viewModel.ChatFilterChanged += OnChatFilterChanged;
            DataContext = _viewModel;
            grdAvatar.DataContext = ownProfile;
            pwdPassword.Password = new string(PasswordMaskFiller, PasswordMaskLength);
            txtNewEmail.IsVisibleChanged += OnNewEmailVisibleChanged;
            Loaded += OnPanelLoaded;
        }

        // The settings are read once the panel is on screen, because a failed read opens the error dialog over it.
        private void OnPanelLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnPanelLoaded;
            _viewModel.LoadAccountSettings();
        }

        private void OnNewEmailVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (txtNewEmail.IsVisible)
            {
                txtNewEmail.Focus();
            }
        }

        // Only a confirmed code changes the email, so closing the screen without one leaves the panel as it was (CU-07).
        private void OnEmailVerificationRequested(object sender, EventArgs e)
        {
            VerifyEmailViewModel emailChange = ((App)Application.Current).CreateEmailChangeVerifyEmailViewModel(_viewModel.PendingEmail);
            emailChange.CodeVerified += OnEmailChangeVerified;
            ShowModal(new GuiVerifyEmail(emailChange));
        }

        private void OnEmailChangeVerified(object sender, EventArgs e)
        {
            _viewModel.CompleteEmailChange();
        }

        private void OnChangePasswordClick(object sender, RoutedEventArgs e)
        {
            PasswordChangeViewModel passwordChange = ((App)Application.Current).CreatePasswordChangeViewModel();
            passwordChange.PasswordUpdated += OnPasswordChangeRequested;
            ShowModal(new GuiNewPassword(passwordChange));
        }

        // The server already emailed the code when this runs, so the code screen opens while the password screen closes (CU-06).
        private void OnPasswordChangeRequested(object sender, EventArgs e)
        {
            PasswordChangeViewModel passwordChange = (PasswordChangeViewModel)sender;
            TwoStepViewModel codeCheck = ((App)Application.Current).CreateTwoStepPasswordChangeViewModel(
                passwordChange.CurrentPassword,
                passwordChange.NewPassword);
            codeCheck.CodeVerified += OnPasswordChanged;
            ShowModal(new GuiTwoStep(codeCheck));
        }

        private void OnPasswordChanged(object sender, EventArgs e)
        {
            _viewModel.CompletePasswordChange();
        }

        // The panel already asked for the code, so the code screen only confirms it and the panel reads the result again (CU-04).
        private void OnTwoStepEnableRequested(object sender, EventArgs e)
        {
            GuiTwoStep twoStep = new GuiTwoStep(((App)Application.Current).CreateTwoStepActivationViewModel());
            twoStep.Closed += OnTwoStepActivationClosed;
            ShowModal(twoStep);
        }

        private void OnTwoStepActivationClosed(object sender, EventArgs e)
        {
            _viewModel.CompleteTwoStepEnable();
        }

        private void OnTwoStepDisableRequested(object sender, EventArgs e)
        {
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.TwoStepDisable);
            confirmDialog.Closed += OnTwoStepDisableConfirmClosed;
            ShowModal(confirmDialog);
        }

        private void OnTwoStepDisableConfirmClosed(object sender, EventArgs e)
        {
            GuiConfirmDialog confirmDialog = (GuiConfirmDialog)sender;
            _viewModel.CompleteTwoStepDisable(confirmDialog.IsConfirmed, confirmDialog.EnteredPassword);
        }

        // The selector applies the language by itself, so only a change made by the player is kept on the server (CU-20).
        // The first selection only shows the active language, and it has no previous item.
        // Without a session the language stays in memory only, because nobody keeps it (CU-20 FA-01).
        private void OnLanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App application = (App)Application.Current;
            bool isPlayerChange = (e.RemovedItems.Count > 0) && (e.AddedItems.Count > 0);
            if (isPlayerChange && (application.SignedInPlayer != null))
            {
                string localeCode = ((CultureOption)e.AddedItems[0]).Code;
                _viewModel.SaveLanguage(application.SignedInPlayer.AccessToken, localeCode);
            }
        }

        // The chat reads the preference of the session, so it follows the change without asking the server again (CU-17 step 10).
        private void OnChatFilterChanged(object sender, EventArgs e)
        {
            ((App)Application.Current).SignedInPlayer.IsChatFilterEnabled = _viewModel.IsChatFilterEnabled;
        }

        private void OnErrorDialogRequested(object sender, ErrorDialogKind kind)
        {
            GuiErrorDialog errorDialog = new GuiErrorDialog(kind);
            if (kind == ErrorDialogKind.SessionExpired)
            {
                errorDialog.Closed += OnSessionExpiredDialogClosed;
            }

            ShowModal(errorDialog);
        }

        // An expired session leaves the lobby the same way as closing the session (CU-20 and CU-03).
        private void OnSessionExpiredDialogClosed(object sender, EventArgs e)
        {
            LeaveLobby();
        }

        private void OnLogOutClick(object sender, RoutedEventArgs e)
        {
            LeaveLobby();
        }
    }
}
