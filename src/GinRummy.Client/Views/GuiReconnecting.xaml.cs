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

        public GuiReconnecting(Func<bool> tryReconnect)
        {
            InitializeComponent();
            _viewModel = ((App)Application.Current).CreateReconnectingViewModel(tryReconnect);
            _viewModel.Reconnected += HandleReconnected;
            _viewModel.GaveUp += HandleGaveUp;
            DataContext = _viewModel;
            Loaded += HandleScreenLoaded;
            Closed += HandleScreenClosed;
        }

        public bool IsReconnected { get; private set; }

        private void HandleScreenLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Start();
        }

        private void HandleReconnected(object sender, EventArgs e)
        {
            IsReconnected = true;
            Close();
        }

        private void HandleGaveUp(object sender, EventArgs e)
        {
            Close();
        }

        private void HandleCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HandleScreenClosed(object sender, EventArgs e)
        {
            _viewModel.Stop();
            _viewModel.Reconnected -= HandleReconnected;
            _viewModel.GaveUp -= HandleGaveUp;
            Loaded -= HandleScreenLoaded;
            Closed -= HandleScreenClosed;
        }
    }
}
