using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using GinRummy.Client.Localization;
using GinRummy.Client.Views;

namespace GinRummy.Client.Converters
{
    // It serves the texts drawn inside the rows of a list, which no window can rebuild one by one.
    // The format string is bound to the localization provider so that the message is rebuilt when the culture changes.
    public sealed class LocalizedFormatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            string format = values.Length > 1 ? values[0] as string : null;
            if (!string.IsNullOrEmpty(format))
            {
                // The binding hands over the culture of the language of the element, which WPF leaves in en-US.
                // The active culture is therefore taken from the provider instead.
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("A formatted message is never written back.");
        }

        private static object FormatArgument(object argument, string argumentFormat, CultureInfo culture)
        {
            object formatted = argument;
            if ((argument == null) || (argument == DependencyProperty.UnsetValue))
            {
                formatted = string.Empty;
            }
            else if (argument is TimeSpan duration)
            {
                formatted = DurationCommon.ToClock(duration, culture);
            }
            else if (!string.IsNullOrEmpty(argumentFormat) && (argument is IFormattable formattable))
            {
                formatted = formattable.ToString(argumentFormat, culture);
            }

            return formatted;
        }
    }
}
