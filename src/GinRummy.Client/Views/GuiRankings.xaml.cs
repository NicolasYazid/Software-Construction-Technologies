using System.Windows;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    public partial class GuiRankings : GuiModalBase
    {
        public GuiRankings()
        {
            InitializeComponent();
            RankingsViewModel viewModel = ((App)Application.Current).CreateRankingsViewModel();
            DataContext = viewModel;
            viewModel.LoadRankingsCommand.Execute(null);
        }

        private void OnTabSelectionChanged(object sender, RoutedEventArgs e)
        {
        }

        private void OnSearchTextChanged(object sender, RoutedEventArgs e)
        {
        }

        private void OnSearchClick(object sender, RoutedEventArgs e)
        {
            txtSearch.Focus();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}