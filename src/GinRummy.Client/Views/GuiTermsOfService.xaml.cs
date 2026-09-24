using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Terms of service screen (P24). Opened from the banned account screen of CU-02 EX-05.
    /// </summary>
    public partial class GuiTermsOfService : GuiWindowBase
    {
        /// <summary>
        /// Builds the terms of service screen.
        /// </summary>
        public GuiTermsOfService()
        {
            InitializeComponent();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
