using System;
using System.Globalization;
using System.Windows.Data;

using GinRummy.Client.Localization;

namespace GinRummy.Client.Converters
{
    // A screen whose text depends on its state binds the key of the text instead of the text itself.
    // The culture of the provider is the second bound value, so the text is looked up again when the culture changes.
    // Any further value is an argument of the text, which is then a format string of the dictionary.
    public sealed class ResourceTextConverter : IMultiValueConverter
    {
        private const int FirstArgumentIndex = 2;

        public bool IsUpperCase { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string text = string.Empty;
            string resourceKey = values.Length > 0 ? values[0] as string : null;
            if (!string.IsNullOrEmpty(resourceKey))
            {
                LocalizationProvider localization = LocalizationProvider.Instance;
                text = localization.GetText(resourceKey);
                if (values.Length > FirstArgumentIndex)
                {
                    object[] arguments = new object[values.Length - FirstArgumentIndex];
                    Array.Copy(values, FirstArgumentIndex, arguments, 0, arguments.Length);
                    text = string.Format(localization.CurrentCulture, text, arguments);
                }

                if (IsUpperCase)
                {
                    text = text.ToUpper(localization.CurrentCulture);
                }
            }

            return text;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("A text looked up by its key is never written back.");
        }
    }
}
