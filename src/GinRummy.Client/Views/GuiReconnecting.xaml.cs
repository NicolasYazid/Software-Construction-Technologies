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
            _viewModel.Reconnected += OnReconnected;
            _viewModel.GaveUp += OnGaveUp;
            DataContext = _viewModel;
            Loaded += OnScreenLoaded;
            Closed += OnScreenClosed;
        }

        public bool IsReconnected { get; private set; }

        private void OnScreenLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Start();
        }

        private void OnReconnected(object sender, EventArgs e)
        {
            IsReconnected = true;
            Close();
        }

        private void OnGaveUp(object sender, EventArgs e)
        {
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _viewModel.Stop();
            _viewModel.Reconnected -= OnReconnected;
            _viewModel.GaveUp -= OnGaveUp;
            Loaded -= OnScreenLoaded;
            Closed -= OnScreenClosed;
        }
    }
}
