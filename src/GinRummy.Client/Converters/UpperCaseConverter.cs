using System;
using System.Globalization;
using System.Windows.Data;

namespace GinRummy.Client.Converters
{
    // The conversion uses the active culture and never the invariant one, because the upper case form of a letter depends on the language.
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Upper case text is never written back.");
        }
    }
}
