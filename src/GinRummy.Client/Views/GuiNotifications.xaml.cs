using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using GinRummy.Client.Models;
using GinRummy.Client.Services;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Notifications screen (P12). Lists the friend requests of CU-13 and answers them through
    /// CU-14 and CU-15. Its second tab is the sanctions screen (P13).
    /// </summary>
    public partial class GuiNotifications : GuiWindowBase
    {
        private readonly ObservableCollection<FriendRequestDto> _friendRequests;

        /// <summary>
        /// Builds the screen with the requests that wait for an answer.
        /// </summary>
        public GuiNotifications()
        {
            InitializeComponent();
            SampleDataService dataService = new SampleDataService();
            _friendRequests = new ObservableCollection<FriendRequestDto>(dataService.GetFriendRequests());
            lstFriendRequests.ItemsSource = _friendRequests;
            RefreshEmptyState();
        }

        private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // The two tabs are two screens of the prototype, so choosing the other one opens
            // it in the same place instead of switching a panel. The selection the tab control
            // makes while it loads is not a choice of the player.
            if (IsLoaded && tabSanctions.IsSelected)
            {
                ReplaceInPlace(new GuiSanctions());
            }
        }

        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            // The friendship is created by the server (CU-14); what the screen does once it
            // confirms is take the request off the list.
            RemoveRequest(sender);
        }

        private void OnDeclineClick(object sender, RoutedEventArgs e)
        {
            RemoveRequest(sender);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RemoveRequest(object sender)
        {
            FriendRequestDto request = ((FrameworkElement)sender).DataContext as FriendRequestDto;
            _friendRequests.Remove(request);
            RefreshEmptyState();
        }

        private void RefreshEmptyState()
        {
            lblNoPendingRequests.Visibility = VisibilityCommon.FromCondition(_friendRequests.Count == 0);
        }
    }
}
