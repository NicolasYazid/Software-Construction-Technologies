using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// New password screen (P08). Serves CU-06 and the last step of CU-08. The title comes
    /// from a different key in each flow, because one key cannot hold two values.
    /// </summary>
    public partial class GuiNewPassword : GuiWindowBase
    {
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
            RefreshFormattedText();
        }

        /// <summary>
        /// Rebuilds the title, which depends on the flow the screen was opened from.
        /// </summary>
        protected override void RefreshFormattedText()
        {
            if (lblTitle != null)
            {
                string key = "NewPassword_LblTitle";
                if (_isChangeFromProfile)
                {
                    key = "NewPassword_LblTitleChange";
                }

                lblTitle.Text = Localization.GetText(key);
            }
        }

        private void OnUpdateClick(object sender, RoutedEventArgs e)
        {
            // Matching the confirmation is the only check the client resolves on its own,
            // because it sends nothing to the server. The strength rules run on the server.
            bool passwordsMatch = pwdNewPassword.Password == pwdConfirmPassword.Password;
            if (passwordsMatch)
            {
                lblErrorMessage.Visibility = Visibility.Collapsed;
                Close();
            }
            else
            {
                lblErrorMessage.Text = Localization.GetText("Error_ValPasswordMismatch");
                lblErrorMessage.Visibility = Visibility.Visible;
            }
        }
    }
}
