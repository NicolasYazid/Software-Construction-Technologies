using System;
using System.Globalization;
using System.IO;

using GinRummy.Domain.Ports.Infrastructure;

namespace GinRummy.Infrastructure
{
    /// <summary>
    /// Fulfils IProfilePictureStorage with a folder of the machine that runs the server. Each player
    /// has one file, named after the player, so a new picture overwrites the previous one of the
    /// same format.
    /// </summary>
    public class FileProfilePictureStorage : IProfilePictureStorage
    {
        private readonly string _folderPath;

        /// <summary>
        /// Builds the storage on a folder, which is created the first time a picture is saved.
        /// </summary>
        /// <param name="folderPath">Folder that keeps the pictures.</param>
        public FileProfilePictureStorage(string folderPath)
        {
            _folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
        }

        /// <summary>
        /// Stores the picture of a player, replacing any previous one of the same format.
        /// </summary>
        /// <param name="playerId">Player the picture belongs to.</param>
        /// <param name="content">Bytes of the image file.</param>
        /// <param name="fileExtension">Extension of the image, with its dot, such as ".png".</param>
        /// <returns>Name of the file, which is what the database keeps.</returns>
        public string Save(int playerId, byte[] content, string fileExtension)
        {
            Directory.CreateDirectory(_folderPath);
            string fileName = playerId.ToString(CultureInfo.InvariantCulture) + fileExtension;
            File.WriteAllBytes(Path.Combine(_folderPath, fileName), content);

            return fileName;
        }

        /// <summary>
        /// Reads a stored picture.
        /// </summary>
        /// <param name="storedPath">Name of the file previously returned by Save.</param>
        /// <returns>Bytes of the image file, or null when the file no longer exists.</returns>
        public byte[] Load(string storedPath)
        {
            string filePath = Path.Combine(_folderPath, storedPath);
            byte[] content = null;
            if (File.Exists(filePath))
            {
                content = File.ReadAllBytes(filePath);
            }

            return content;
        }

        /// <summary>
        /// Deletes a stored picture. Does nothing when the file no longer exists.
        /// </summary>
        /// <param name="storedPath">Name of the file previously returned by Save.</param>
        public void Delete(string storedPath)
        {
            string filePath = Path.Combine(_folderPath, storedPath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
