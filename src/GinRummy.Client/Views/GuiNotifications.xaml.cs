using System.Windows;
using System.Windows.Controls;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Notifications screen (P12). Lists the friend requests of CU-13 and answers them through
    /// CU-14 and CU-15. Its second tab is the sanctions screen (P13).
    /// </summary>
    public partial class GuiNotifications : GuiModalBase
    {
        public GuiNotifications()
        {
            InitializeComponent();
            DataContext = new NotificationsViewModel(((App)Application.Current).SampleData);
        }

        private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The two tabs are two screens of the prototype, so choosing the other tab opens its screen here instead of switching a panel.
            // The selection the tab control makes while it loads is not a choice of the player.
            if (IsLoaded && tabSanctions.IsSelected)
            {
                NavigateTo(new GuiSanctions());
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
