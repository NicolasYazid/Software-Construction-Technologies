using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Report player dialog (P17). Implements the screen of CU-18: the player picks one reason
    /// of the catalogue, may describe what happened and sends the report.
    /// </summary>
    public partial class GuiReportPlayer : GuiWindowBase
    {
        /// <summary>
        /// Builds the dialog for the player being reported.
        /// </summary>
        /// <param name="username">Name of the reported player, which is never translated.</param>
        public GuiReportPlayer(string username)
        {
            InitializeComponent();
            lblUsername.Text = username;
        }

        /// <summary>
        /// Gets whether the player sent the report instead of cancelling it.
        /// </summary>
        public bool IsSubmitted { get; private set; }

        private void OnReasonChecked(object sender, RoutedEventArgs e)
        {
            lblNoCategorySelected.Visibility = Visibility.Collapsed;
        }

        private void OnSubmitClick(object sender, RoutedEventArgs e)
        {
            bool isReasonSelected = radReasonOffensiveLanguage.IsChecked == true
                || radReasonHarassmentOrThreats.IsChecked == true
                || radReasonCheatingOrExploits.IsChecked == true
                || radReasonSpam.IsChecked == true;
            if (isReasonSelected)
            {
                IsSubmitted = true;
                Close();
            }
            else
            {
                // Without a reason no report is created (CU-18 FA-03).
                lblNoCategorySelected.Visibility = Visibility.Visible;
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
