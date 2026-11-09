namespace GinRummy.Domain.Ports.PlayerCustomization
{
    /// <summary>
    /// Contract the game's logic uses to keep profile pictures, without knowing whether they
    /// live on disk or elsewhere. The database stores only the path this contract returns.
    /// </summary>
    public interface IProfilePictureStorage
    {
        /// <summary>
        /// Stores the picture of a player, replacing any previous one.
        /// </summary>
        /// <param name="playerAccountId">Player the picture belongs to.</param>
        /// <param name="content">Bytes of the image file.</param>
        /// <param name="fileExtension">Extension of the image, with its dot, such as ".png".</param>
        /// <returns>The path to save in the profile, at most 255 characters.</returns>
        string Save(int playerAccountId, byte[] content, string fileExtension);

        /// <summary>
        /// Reads a stored picture.
        /// </summary>
        /// <param name="storedPath">Path previously returned by Save.</param>
        /// <returns>Bytes of the image file, or null when the path no longer exists.</returns>
        byte[] Load(string storedPath);

        /// <summary>
        /// Deletes a stored picture. Does nothing when the path no longer exists.
        /// </summary>
        /// <param name="storedPath">Path previously returned by Save.</param>
        void Delete(string storedPath);
    }
}
