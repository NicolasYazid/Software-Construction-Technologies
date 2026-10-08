using System;
using System.Globalization;
using System.Windows.Data;

namespace GinRummy.Client.Converters
{
    // The separators and the percent sign change with the culture (CU-20 step 6).
    // The culture is a bound value of the localization provider so that the number is formatted again when the culture changes.
    public sealed class CultureNumberConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            IFormattable number = values.Length > 1 ? values[0] as IFormattable : null;
            CultureInfo activeCulture = values.Length > 1 ? values[1] as CultureInfo : null;
            if ((number != null) && (activeCulture != null))
            {
                result = number.ToString(parameter as string, activeCulture);
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("A formatted number is never written back.");
        }
    }
}
