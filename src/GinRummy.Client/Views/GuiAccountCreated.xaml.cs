using System.Windows;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Account created screen (P04). Closes the sign-up flow of CU-01.
    /// </summary>
    public partial class GuiAccountCreated : GuiWindowBase
    {
        /// <summary>
        /// Builds the account created screen.
        /// </summary>
        public GuiAccountCreated()
        {
            InitializeComponent();
        }

        private void OnLogInClick(object sender, RoutedEventArgs e)
        {
            NavigateTo(new GuiLogIn());
        }
    }
}
