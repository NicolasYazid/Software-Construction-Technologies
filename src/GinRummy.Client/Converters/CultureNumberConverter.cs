using System;
using System.Globalization;
using System.Windows.Data;

namespace GinRummy.Client.Converters
{
    /// <summary>
    /// Formats a number of a data item with the active culture, for the counters and the
    /// percentages drawn inside the rows of a list, whose separators and percent sign change
    /// with the culture (CU-20 step 6). The first value of the binding is the number; the
    /// second is the active culture, bound to the localization provider so that the number is
    /// formatted again when the culture changes. The parameter is the numeric format, such as
    /// N0 or P0.
    /// </summary>
    public sealed class CultureNumberConverter : IMultiValueConverter
    {
        /// <summary>
        /// Returns the number formatted with the active culture.
        /// </summary>
        /// <param name="values">The number followed by the active culture.</param>
        /// <param name="targetType">Type requested by the binding.</param>
        /// <param name="parameter">Numeric format, such as N0 or P0.</param>
        /// <param name="culture">Culture supplied by the binding, which is not the active
        /// one.</param>
        /// <returns>The formatted number, or an empty string while either value is missing.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            IFormattable number = values.Length > 1 ? values[0] as IFormattable : null;
            CultureInfo activeCulture = values.Length > 1 ? values[1] as CultureInfo : null;
            if (number != null && activeCulture != null)
            {
                result = number.ToString(parameter as string, activeCulture);
            }

            return result;
        }

        /// <summary>
        /// Not supported: a formatted number is only shown, never written back.
        /// </summary>
        /// <param name="value">Not used.</param>
        /// <param name="targetTypes">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Never returns: the operation is not supported.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("A formatted number is never written back.");
        }
    }
}
