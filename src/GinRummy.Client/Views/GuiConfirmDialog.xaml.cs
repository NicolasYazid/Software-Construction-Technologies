using System.Windows;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Confirmation dialog (P23). Asks before the destructive actions of CU-03, CU-05, CU-16,
    /// CU-26, CU-31 and CU-32, each one with its own title, warning and confirming button.
    /// </summary>
    public partial class GuiConfirmDialog : GuiModalBase
    {
        private const string MessageStyleKey = "StyDialogMessage";
        private const string WarningStyleKey = "StyDialogWarning";

        private readonly ConfirmDialogViewModel _viewModel;

        public GuiConfirmDialog(ConfirmDialogKind kind)
        {
            InitializeComponent();
            _viewModel = new ConfirmDialogViewModel(kind);
            DataContext = _viewModel;
            ApplyMessageStyle();
        }

        public bool IsConfirmed { get; private set; }

        public string EnteredPassword
        {
            get { return _viewModel.Password; }
        }

        private void ApplyMessageStyle()
        {
            string messageStyleKey = MessageStyleKey;
            if (_viewModel.IsWarning)
            {
                messageStyleKey = WarningStyleKey;
            }

            lblMessage.Style = (Style)FindResource(messageStyleKey);
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
