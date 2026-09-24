using System.Windows;

using GinRummy.Client.Services;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// House rules screen (P19). Implements CU-21: the twenty rules and the glossary in the
    /// active language, with the values of the game quoted from its configuration instead of
    /// written into the translated text. It opens over the screen that asked for it and returns
    /// to it when closed.
    /// </summary>
    public partial class GuiHouseRules : GuiWindowBase
    {
        /// <summary>
        /// Builds the screen with the values of the game the rules quote.
        /// </summary>
        public GuiHouseRules()
        {
            InitializeComponent();
            SampleDataService dataService = new SampleDataService();
            DataContext = dataService.GetGameRules();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
