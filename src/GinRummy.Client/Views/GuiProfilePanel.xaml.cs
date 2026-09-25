using System.Collections.ObjectModel;
using System.Windows;

using GinRummy.Client.Models;
using GinRummy.Client.Services;

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

        private readonly ObservableCollection<LinkedAccountDto> _linkedAccounts;

        /// <summary>
        /// Builds the panel with the current settings of the account.
        /// </summary>
        public GuiProfilePanel()
        {
            InitializeComponent();
            SampleDataService dataService = new SampleDataService();
            AccountSettingsDto settings = dataService.GetAccountSettings();
            lblEmailAddress.Text = settings.Email;
            pwdPassword.Password = new string(PasswordMaskFiller, PasswordMaskLength);
            tglTwoStep.IsChecked = settings.IsTwoStepEnabled;
            sldMasterVolume.Value = settings.MasterVolume;
            tglChatFilter.IsChecked = settings.IsChatFilterEnabled;
            _linkedAccounts = new ObservableCollection<LinkedAccountDto>(settings.LinkedAccounts);
            lstLinkedAccounts.ItemsSource = _linkedAccounts;
        }

        private void OnChangeEmailClick(object sender, RoutedEventArgs e)
        {
            txtNewEmail.Clear();
            lblErrorMessage.Visibility = Visibility.Collapsed;
            lblChangeEmailTitle.Visibility = Visibility.Visible;
            txtNewEmail.Focus();
        }

        private void OnCancelEmailClick(object sender, RoutedEventArgs e)
        {
            lblChangeEmailTitle.Visibility = Visibility.Collapsed;
        }

        private void OnSaveEmailClick(object sender, RoutedEventArgs e)
        {
            string newEmail = txtNewEmail.Text.Trim();

            // Only the empty field is caught here (CU-07 FA-02); the format and the uniqueness
            // of the address are checked by the server before the code is sent.
            if (newEmail.Length == 0)
            {
                lblErrorMessage.Visibility = Visibility.Visible;
            }
            else
            {
                lblChangeEmailTitle.Visibility = Visibility.Collapsed;
                GuiVerifyEmail verifyEmail = new GuiVerifyEmail(VerificationPurpose.EmailChange, newEmail);
                verifyEmail.Closed += (source, arguments) =>
                {
                    lblEmailAddress.Text = newEmail;
                    lblEmailChanged.Visibility = Visibility.Visible;
                };
                ShowModal(verifyEmail);
            }
        }

        private void OnChangePasswordClick(object sender, RoutedEventArgs e)
        {
            GuiNewPassword newPassword = new GuiNewPassword(true);
            ShowModal(newPassword);
        }

        private void OnTwoStepClick(object sender, RoutedEventArgs e)
        {
            lblTwoStepDisabled.Visibility = Visibility.Collapsed;
            if (tglTwoStep.IsChecked == true)
            {
                EnableTwoStep();
            }
            else
            {
                DisableTwoStep();
            }
        }

        private void OnLinkClick(object sender, RoutedEventArgs e)
        {
            ReplaceLinkedAccount(sender, true);
        }

        private void OnUnlinkClick(object sender, RoutedEventArgs e)
        {
            ReplaceLinkedAccount(sender, false);
        }

        private void OnLogOutClick(object sender, RoutedEventArgs e)
        {
            // Closing the session closes the lobby too, which leaves the player in the main menu
            // that opened it (CU-03 step 5).
            GuiWindowBase lobby = Host;
            Close();
            if (lobby != null)
            {
                lobby.Close();
            }
        }

        private void EnableTwoStep()
        {
            // The switch turns on only once the code of CU-04 is confirmed in the verification
            // screen, which is the one that talks to the server.
            GuiTwoStep twoStep = new GuiTwoStep(TwoStepPurpose.EnableTwoStep);
            ShowModal(twoStep);
        }

        private void DisableTwoStep()
        {
            GuiConfirmDialog confirmDialog = new GuiConfirmDialog(ConfirmDialogKind.TwoStepDisable);

            // Cancelling returns the switch to where it was (CU-05 FA-02).
            confirmDialog.Closed += (source, arguments) =>
            {
                if (confirmDialog.IsConfirmed)
                {
                    lblTwoStepDisabled.Visibility = Visibility.Visible;
                }
                else
                {
                    tglTwoStep.IsChecked = true;
                }
            };
            ShowModal(confirmDialog);
        }

        private void ReplaceLinkedAccount(object sender, bool isLinked)
        {
            LinkedAccountDto account = ((FrameworkElement)sender).DataContext as LinkedAccountDto;
            int index = _linkedAccounts.IndexOf(account);
            if (index >= 0)
            {
                LinkedAccountDto updated = new LinkedAccountDto();
                updated.PlatformName = account.PlatformName;
                updated.IsLinked = isLinked;
                _linkedAccounts[index] = updated;
            }
        }
    }
}
