using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// New password screen (P08). Serves CU-06 and the last step of CU-08. Both the title and
    /// the current password field depend on the flow the screen was opened from, because the
    /// recovery flow reaches this screen precisely when the player no longer knows the
    /// password that is in force.
    /// </summary>
    public partial class GuiNewPassword : GuiWindowBase
    {
        private const string TitleKeyRecovery = "NewPassword_LblTitle";
        private const string TitleKeyChange = "NewPassword_LblTitleChange";
        private const string PasswordMismatchKey = "Error_ValPasswordMismatch";

        private static readonly Thickness NewPasswordLabelAfterText = new Thickness(0, 28, 0, 8);
        private static readonly Thickness NewPasswordLabelAfterField = new Thickness(0, 22, 0, 8);

        private readonly bool _isChangeFromProfile;

        /// <summary>
        /// Builds the screen for the recovery flow.
        /// </summary>
        public GuiNewPassword()
            : this(false)
        {
        }

        /// <summary>
        /// Builds the screen for a given flow.
        /// </summary>
        /// <param name="isChangeFromProfile">True when the player changes the password from
        /// the profile panel, false when the password is being recovered.</param>
        public GuiNewPassword(bool isChangeFromProfile)
        {
            InitializeComponent();
            _isChangeFromProfile = isChangeFromProfile;
            ApplyFlowLayout();
            RefreshFormattedText();
        }

        /// <summary>
        /// Rebuilds the heading and the window title, which come from a different key in each
        /// flow because one key cannot hold two values.
        /// </summary>
        protected override void RefreshFormattedText()
        {
            if (lblTitle != null)
            {
                string titleKey = ResolveTitleKey();
                lblTitle.Text = Localization.GetText(titleKey);
                Title = Localization.GetText(titleKey);
            }
        }

        private string ResolveTitleKey()
        {
            string titleKey = TitleKeyRecovery;
            if (_isChangeFromProfile)
            {
                titleKey = TitleKeyChange;
            }

            return titleKey;
        }

        private void ApplyFlowLayout()
        {
            Visibility currentPasswordVisibility = Visibility.Collapsed;
            Thickness newPasswordLabelMargin = NewPasswordLabelAfterText;

            if (_isChangeFromProfile)
            {
                currentPasswordVisibility = Visibility.Visible;
                newPasswordLabelMargin = NewPasswordLabelAfterField;
            }

            lblCurrentPassword.Visibility = currentPasswordVisibility;
            brdCurrentPassword.Visibility = currentPasswordVisibility;
            lblNewPassword.Margin = newPasswordLabelMargin;
        }

        private void OnUpdateClick(object sender, RoutedEventArgs e)
        {
            // Matching the confirmation is the only check the client resolves on its own,
            // because it sends nothing to the server. The strength rules and the current
            // password itself are verified on the server, as CU-06 requires.
            bool passwordsMatch = pwdNewPassword.Password == pwdConfirmPassword.Password;
            if (passwordsMatch)
            {
                lblErrorMessage.Visibility = Visibility.Collapsed;
                Close();
            }
            else
            {
                lblErrorMessage.Text = Localization.GetText(PasswordMismatchKey);
                lblErrorMessage.Visibility = Visibility.Visible;
            }
        }
    }
}
