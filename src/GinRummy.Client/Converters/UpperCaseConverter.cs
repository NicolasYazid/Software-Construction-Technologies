using System;
using System.Globalization;
using System.Windows.Data;

namespace GinRummy.Client.Converters
{
    // Converts visible text to upper case for the controls the prototype defines that way. The
    // conversion runs with the active culture and never with the invariant one, because the
    // upper case form of a letter depends on the language.
    public sealed class UpperCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            string result = string.Empty;
            if (!string.IsNullOrEmpty(text))
            {
                result = text.ToUpper(CultureInfo.CurrentCulture);
            }

            return result;
        }

        // Not supported: the upper case form never writes back into the resource.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Upper case text is never written back.");
        }
    }
}
