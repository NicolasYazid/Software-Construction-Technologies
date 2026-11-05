using System.Linq;

namespace GinRummy.Domain.Services
{
    /// <summary>
    /// Rules a profile picture must meet: a JPG or PNG image of at most 2 MB.
    /// </summary>
    public static class ProfilePictureCommon
    {
        /// <summary>
        /// Largest picture the game accepts, in bytes.
        /// </summary>
        public const int MaxSizeInBytes = 2 * 1024 * 1024;

        private const string PngExtension = ".png";
        private const string JpgExtension = ".jpg";

        private static readonly byte[] _pngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] _jpgSignature = { 0xFF, 0xD8, 0xFF };

        /// <summary>
        /// Tells the file extension of a picture from its first bytes, because the name of the file
        /// is chosen by whoever sends it.
        /// </summary>
        /// <param name="picture">Bytes of the image file.</param>
        /// <returns>".png" or ".jpg", or null when the picture is empty, too big or of another format.</returns>
        public static string FindFileExtension(byte[] picture)
        {
            string fileExtension = null;
            bool hasAllowedSize = (picture != null) && (picture.Length > 0) && (picture.Length <= MaxSizeInBytes);
            if (hasAllowedSize && StartsWith(picture, _pngSignature))
            {
                fileExtension = PngExtension;
            }
            else if (hasAllowedSize && StartsWith(picture, _jpgSignature))
            {
                fileExtension = JpgExtension;
            }

            return fileExtension;
        }

        private static bool StartsWith(byte[] picture, byte[] signature)
        {
            return (picture.Length >= signature.Length) && picture.Take(signature.Length).SequenceEqual(signature);
        }
    }
}
