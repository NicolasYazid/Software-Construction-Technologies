using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GinRummy.Client.Converters
{
    // Shows the guide text of a field only while that field is empty. It exists because the
    // guide text is a localized string of its own and must not be written inside the control as
    // fixed text.
    public sealed class EmptyTextToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            Visibility visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(text))
            {
                visibility = Visibility.Visible;
            }

            return visibility;
        }

        // Not supported: the guide text never writes back into the field.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("The guide text does not write back into the field.");
        }
    }
}
