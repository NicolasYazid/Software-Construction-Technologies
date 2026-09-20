using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GinRummy.Client.Converters
{
    /// <summary>
    /// Shows the guide text of a field only while that field is empty. It exists because
    /// the guide text is a localized string of its own and must not be written inside the
    /// control as fixed text.
    /// </summary>
    public sealed class EmptyTextToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts the content of a field into the visibility of its guide text.
        /// </summary>
        /// <param name="value">Current content of the field.</param>
        /// <param name="targetType">Type requested by the binding.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Culture supplied by the binding.</param>
        /// <returns>Visible while the field is empty, collapsed otherwise.</returns>
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

        /// <summary>
        /// Not supported: the guide text never writes back into the field.
        /// </summary>
        /// <param name="value">Not used.</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Never returns: the operation is not supported.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("The guide text does not write back into the field.");
        }
    }
}
