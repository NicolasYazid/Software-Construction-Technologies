using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using GinRummy.Client.Models;
using GinRummy.Client.Services;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Sanctions screen (P13). Implements CU-33: lists the sanctions of the player, marks the
    /// ones in force and shows how long they have left. Its first tab is the notifications
    /// screen (P12).
    /// </summary>
    public partial class GuiSanctions : GuiWindowBase
    {
        /// <summary>
        /// Builds the screen with the sanctions of the player.
        /// </summary>
        public GuiSanctions()
        {
            InitializeComponent();
            LoadSanctions();
        }

        /// <summary>
        /// Loads the sanctions again, because the name of their reason arrives already
        /// translated and has to be asked for in the new language (CU-33 step 3).
        /// </summary>
        protected override void RefreshFormattedText()
        {
            if (lstSanctions != null)
            {
                LoadSanctions();
            }
        }

        private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && tabFriendRequests.IsSelected)
            {
                ReplaceInPlace(new GuiNotifications());
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadSanctions()
        {
            SampleDataService dataService = new SampleDataService();
            IList<SanctionDto> sanctions = dataService.GetSanctions();
            lstSanctions.ItemsSource = sanctions;
            Visibility emptyStateVisibility = Visibility.Collapsed;
            if (sanctions.Count == 0)
            {
                emptyStateVisibility = Visibility.Visible;
            }

            lblEmptyState.Visibility = emptyStateVisibility;
        }
    }
}
