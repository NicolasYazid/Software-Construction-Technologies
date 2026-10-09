using System;
using System.IO;
using System.Windows.Media.Imaging;

using Microsoft.Extensions.Logging;

namespace GinRummy.Client.Controllers
{
    public class EditProfileController
    {
        private const long BytesPerMegabyte = 1024L * 1024L;
        private const long MaxImageMegabytes = 2L;
        private const long MaxImageBytes = MaxImageMegabytes * BytesPerMegabyte;

        private readonly ILogger<EditProfileController> _logger;

        public EditProfileController(ILogger<EditProfileController> logger)
        {
            _logger = logger;
        }

        // A file that is too large or cannot be decoded returns null instead of throwing, so the screen decides what to show.
        public BitmapImage LoadPicture(string path)
        {
            BitmapImage picture = null;
            FileInfo file = new FileInfo(path);
            if (file.Length <= MaxImageBytes)
            {
                picture = DecodePicture(path);
            }

            return picture;
        }

        private BitmapImage DecodePicture(string path)
        {
            BitmapImage picture = new BitmapImage();
            try
            {
                picture.BeginInit();
                picture.CacheOption = BitmapCacheOption.OnLoad;
                picture.UriSource = new Uri(path);
                picture.EndInit();
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning(ex, "A profile picture was rejected because its format is not supported.");
                picture = null;
            }
            catch (FileFormatException ex)
            {
                _logger.LogWarning(ex, "A profile picture was rejected because its file could not be decoded.");
                picture = null;
            }

            return picture;
        }
    }
}
