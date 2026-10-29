using System.Windows;
using System.Windows.Controls;

namespace GinRummy.Client.Controls
{
    // A PasswordBox keeps its value out of the dependency property system so that the value never sits in a binding by accident.
    // A screen whose view model needs the value opts in with this attached property, field by field.
    public static class PasswordBindingCommon
    {
        // The default value is null so that the empty password a view model starts with still reaches the field and attaches the handler.
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBindingCommon),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

        // WPF resolves an attached property from XAML through static Get and Set accessors, so these cannot be a property.
        public static string GetBoundPassword(DependencyObject element)
        {
            return (string)element.GetValue(BoundPasswordProperty);
        }

        public static void SetBoundPassword(DependencyObject element, string value)
        {
            element.SetValue(BoundPasswordProperty, value);
        }

        private static void OnBoundPasswordChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordField = element as PasswordBox;
            if (passwordField != null)
            {
                passwordField.PasswordChanged -= OnPasswordChanged;
                string password = (e.NewValue as string) ?? string.Empty;
                if (passwordField.Password != password)
                {
                    passwordField.Password = password;
                }

                passwordField.PasswordChanged += OnPasswordChanged;
            }
        }

        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordField = (PasswordBox)sender;
            SetBoundPassword(passwordField, passwordField.Password);
        }
    }
}
