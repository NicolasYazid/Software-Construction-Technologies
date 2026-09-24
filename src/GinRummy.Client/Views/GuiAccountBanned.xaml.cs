using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Banned account screen (P10). Serves CU-02 EX-05.
    /// </summary>
    public partial class GuiAccountBanned : GuiWindowBase
    {
        /// <summary>
        /// Builds the screen for a permanent ban.
        /// </summary>
        /// <param name="reasonName">Name of the reason of the ban, already in the active
        /// language.</param>
        public GuiAccountBanned(string reasonName)
        {
            InitializeComponent();
            lblReasonBan.Text = reasonName;
        }

        private void OnLogOutClick(object sender, RoutedEventArgs e)
        {
            // A banned account never gets a session (CU-02 EX-05), so leaving only takes the
            // player back to the screen that opened this one.
            Close();
        }

        private void OnTermsOfServiceClick(object sender, RoutedEventArgs e)
        {
            GuiTermsOfService termsOfService = new GuiTermsOfService();
            termsOfService.Owner = this;
            termsOfService.ShowDialog();
        }
    }
}
