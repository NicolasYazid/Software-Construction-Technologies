using System;
using System.Windows;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Suspended account screen (P09). Serves CU-02 EX-04, CU-22 EX-05, CU-23 EX-09 and
    /// CU-33 FA-04.
    /// </summary>
    public partial class GuiAccountSuspended : GuiModalBase
    {
        private readonly AccountSuspendedViewModel _viewModel;

        public GuiAccountSuspended(string reasonName, TimeSpan remainingTime)
        {
            InitializeComponent();
            _viewModel = new AccountSuspendedViewModel(reasonName, remainingTime);
            DataContext = _viewModel;
            Closed += OnScreenClosed;
        }

        private void OnLogOutClick(object sender, RoutedEventArgs e)
        {
            // Leaving only closes the screen because a suspended account never gets a session to end (CU-02 EX-04).
            Close();
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.Countdown.Stop();
            Closed -= OnScreenClosed;
        }
    }
}
