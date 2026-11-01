namespace GinRummy.Domain.Ports.Infrastructure
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
        /// <param name="playerId">Player the picture belongs to.</param>
        /// <param name="content">Bytes of the image file.</param>
        /// <param name="fileExtension">Extension of the image, with its dot, such as ".png".</param>
        /// <returns>The path to save in the profile, at most 255 characters.</returns>
        string Save(int playerId, byte[] content, string fileExtension);

        /// <summary>
        /// Deletes a stored picture. Does nothing when the path no longer exists.
        /// </summary>
        /// <param name="storedPath">Path previously returned by Save.</param>
        void Delete(string storedPath);
    }
}
