using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;

namespace GinRummy.Client.Converters
{
    // A player without a picture has an empty one, so the image stays blank and the default avatar behind it shows (CU-28 EX-04).
    public sealed class PictureSourceConverter : IValueConverter
    {
        private static readonly ImageSourceConverter s_imageSourceConverter = new ImageSourceConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] picture = value as byte[];
            ImageSource source = null;
            if ((picture != null) && (picture.Length > 0))
            {
                try
                {
                    source = (ImageSource)s_imageSourceConverter.ConvertFrom(picture);
                }
                catch (NotSupportedException)
                {
                    // A format this computer cannot decode keeps the default avatar instead of closing the client.
                }
                catch (FileFormatException)
                {
                    // A damaged picture keeps the default avatar instead of closing the client.
                }
            }

            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("A picture is never written back from the screen.");
        }
    }
}
