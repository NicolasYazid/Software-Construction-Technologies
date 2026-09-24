using System.Windows;
using System.Windows.Controls;

namespace GinRummy.Client.Views
{
    // Clear view of a password field. A PasswordBox never draws its characters, so the clear
    // view is a text field laid over it that holds the value while it is shown and hands it
    // back when the player hides it again. Only one of the two holds the value at a time.
    internal static class PasswordRevealCommon
    {
        internal static void Toggle(PasswordBox passwordField, TextBox clearField)
        {
            bool isRevealed = clearField.Visibility == Visibility.Visible;
            if (isRevealed)
            {
                passwordField.Password = clearField.Text;
                clearField.Clear();
                clearField.Visibility = Visibility.Collapsed;
                passwordField.Visibility = Visibility.Visible;
                passwordField.Focus();
            }
            else
            {
                clearField.Text = passwordField.Password;
                clearField.CaretIndex = clearField.Text.Length;
                passwordField.Clear();
                passwordField.Visibility = Visibility.Collapsed;
                clearField.Visibility = Visibility.Visible;
                clearField.Focus();
            }
        }

        internal static string Read(PasswordBox passwordField, TextBox clearField)
        {
            string password = passwordField.Password;
            if (clearField.Visibility == Visibility.Visible)
            {
                password = clearField.Text;
            }

            return password;
        }
    }
}
