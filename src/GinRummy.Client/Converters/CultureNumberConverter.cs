using System;
using System.Globalization;
using System.Windows.Data;

namespace GinRummy.Client.Converters
{
    // Formats a number of a data item with the active culture, for the counters and percentages drawn inside the rows of a list.
    // Their separators and percent sign change with the culture (CU-20 step 6).
    // The first value of the binding is the number.
    // The second is the active culture, bound to the localization provider so that the number is formatted again when the culture changes.
    // The parameter is the numeric format, such as N0 or P0.
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

        // Not supported: a formatted number is only shown, never written back.
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("A formatted number is never written back.");
        }
    }
}
