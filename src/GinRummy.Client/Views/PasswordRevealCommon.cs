using System.Windows;
using System.Windows.Controls;

namespace GinRummy.Client.Views
{
    // A PasswordBox never draws its characters, so the clear view of a password is a text field laid over it.
    // The field that becomes visible takes the value of the one that was shown, so the toggle never loses what the player typed.
    internal static class PasswordRevealCommon
    {
        internal static void Toggle(PasswordBox passwordField, TextBox clearField)
        {
            bool isRevealed = clearField.Visibility == Visibility.Visible;
            if (isRevealed)
            {
                passwordField.Password = clearField.Text;
                clearField.Visibility = Visibility.Collapsed;
                passwordField.Visibility = Visibility.Visible;
                passwordField.Focus();
            }
            else
            {
                clearField.Text = passwordField.Password;
                clearField.CaretIndex = clearField.Text.Length;
                passwordField.Visibility = Visibility.Collapsed;
                clearField.Visibility = Visibility.Visible;
                clearField.Focus();
            }
        }
    }
}
