using System;
using System.Windows;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Reconnection notice (P25). Opened by CU-26 EX-03 and EX-05 and by the NET-03
    /// exceptions of the rest of the use cases.
    /// </summary>
    public partial class GuiReconnecting : GuiModalBase
    {
        private readonly ReconnectingViewModel _viewModel;

        public GuiReconnecting()
        {
            InitializeComponent();
            _viewModel = new ReconnectingViewModel();
            DataContext = _viewModel;
            Closed += OnScreenClosed;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.Countdown.Stop();
            Closed -= OnScreenClosed;
        }
    }
}
