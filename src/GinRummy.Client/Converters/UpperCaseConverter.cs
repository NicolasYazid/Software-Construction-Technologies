using System;
using System.Globalization;
using System.Windows.Data;

namespace GinRummy.Client.Converters
{
    /// <summary>
    /// Converts visible text to upper case for the controls the prototype defines that way.
    /// The conversion runs with the active culture and never with the invariant one, because
    /// the upper case form of a letter depends on the language.
    /// </summary>
    public sealed class UpperCaseConverter : IValueConverter
    {
        /// <summary>
        /// Returns the text in upper case.
        /// </summary>
        /// <param name="value">Text resolved from the resource file.</param>
        /// <param name="targetType">Type requested by the binding.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Culture supplied by the binding.</param>
        /// <returns>The text in upper case, or an empty string when there is no text.</returns>
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

        /// <summary>
        /// Not supported: the upper case form never writes back into the resource.
        /// </summary>
        /// <param name="value">Not used.</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Never returns: the operation is not supported.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Upper case text is never written back.");
        }
    }
}
