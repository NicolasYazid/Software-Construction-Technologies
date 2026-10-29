using System.Windows;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// House rules screen (P19). Implements CU-21: the twenty rules and the glossary in the
    /// active language, with the values of the game quoted from its configuration instead of
    /// written into the translated text. It opens over the screen that asked for it and returns
    /// to it when closed.
    /// </summary>
    public partial class GuiHouseRules : GuiModalBase
    {
        public GuiHouseRules()
        {
            InitializeComponent();
            DataContext = new HouseRulesViewModel(((App)Application.Current).SampleData);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
