using System.Windows;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Error dialog (P22). Presents the data, network, session and game exceptions that the
    /// use cases send to it, each one with its own title, message and actions.
    /// </summary>
    public partial class GuiErrorDialog : GuiModalBase
    {
        public GuiErrorDialog(ErrorDialogKind kind)
        {
            InitializeComponent();
            DataContext = new ErrorDialogViewModel(kind);
        }

        public bool IsActionChosen { get; private set; }

        private void OnActionClick(object sender, RoutedEventArgs e)
        {
            IsActionChosen = true;
            Close();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
