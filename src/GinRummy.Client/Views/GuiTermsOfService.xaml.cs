using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Terms of service screen (P24). Opened from the banned account screen of CU-02 EX-05.
    /// </summary>
    public partial class GuiTermsOfService : GuiModalBase
    {
        public GuiTermsOfService()
        {
            InitializeComponent();
        }

        private void HandleCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
