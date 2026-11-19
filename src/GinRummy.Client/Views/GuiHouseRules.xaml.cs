using System.Windows;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    public partial class GuiHouseRules : GuiModalBase
    {
        public GuiHouseRules()
        {
            InitializeComponent();
            DataContext = ((App)Application.Current).CreateHouseRulesViewModel();
        }

        private void HandleCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
