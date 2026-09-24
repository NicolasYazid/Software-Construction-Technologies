using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using GinRummy.Client.Localization;
using GinRummy.Client.Views;

namespace GinRummy.Client.Converters
{
    /// <summary>
    /// Builds a visible message from a format string of the dictionary and the values of a
    /// data item, for the texts drawn inside the rows of a list, which no window can rebuild
    /// one by one. The first value of the binding is the format string, bound to the
    /// localization provider so that the message is rebuilt when the culture changes; the
    /// rest are its arguments. The parameter, when present, is the format applied to every
    /// argument that accepts one, such as t for the time of a chat message.
    /// </summary>
    public sealed class LocalizedFormatConverter : IMultiValueConverter
    {
        /// <summary>
        /// Returns the message with its arguments formatted with the active culture.
        /// </summary>
        /// <param name="values">The format string followed by its arguments.</param>
        /// <param name="targetType">Type requested by the binding.</param>
        /// <param name="parameter">Optional format for the arguments, such as t or d.</param>
        /// <param name="culture">Culture supplied by the binding, which is not the active
        /// one.</param>
        /// <returns>The message, or an empty string while the format is not resolved.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            string format = values.Length > 1 ? values[0] as string : null;
            if (!string.IsNullOrEmpty(format))
            {
                // The binding hands over the culture of the language of the element, which WPF
                // leaves in en-US, so the active one is taken from the provider instead.
                CultureInfo activeCulture = LocalizationProvider.Instance.CurrentCulture;
                string argumentFormat = parameter as string;
                object[] arguments = new object[values.Length - 1];
                for (int index = 1; index < values.Length; index++)
                {
                    arguments[index - 1] = FormatArgument(values[index], argumentFormat, activeCulture);
                }

                result = string.Format(activeCulture, format, arguments);
            }

            return result;
        }

        /// <summary>
        /// Not supported: a message built from a format string never writes back.
        /// </summary>
        /// <param name="value">Not used.</param>
        /// <param name="targetTypes">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Never returns: the operation is not supported.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("A formatted message is never written back.");
        }

        private static object FormatArgument(object argument, string argumentFormat, CultureInfo culture)
        {
            object formatted = argument;
            if (argument == null || argument == DependencyProperty.UnsetValue)
            {
                formatted = string.Empty;
            }
            else if (argument is TimeSpan duration)
            {
                formatted = DurationCommon.ToClock(duration, culture);
            }
            else if (!string.IsNullOrEmpty(argumentFormat) && argument is IFormattable formattable)
            {
                formatted = formattable.ToString(argumentFormat, culture);
            }

            return formatted;
        }
    }
}
