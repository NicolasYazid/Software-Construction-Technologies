using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Banned account screen (P10). Serves CU-02 EX-05.
    /// </summary>
    public partial class GuiAccountBanned : GuiModalBase
    {
        public GuiAccountBanned(string reasonName)
        {
            InitializeComponent();
            lblReasonBan.Text = reasonName;
        }

        private void OnLogOutClick(object sender, RoutedEventArgs e)
        {
            // Leaving only closes the screen because a banned account never gets a session to end (CU-02 EX-05).
            Close();
        }

        private void OnTermsOfServiceClick(object sender, RoutedEventArgs e)
        {
            GuiTermsOfService termsOfService = new GuiTermsOfService();
            ShowModal(termsOfService);
        }
    }
}
