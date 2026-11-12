namespace GinRummy.Domain.Ports.PlayerCustomization
{
    public interface IProfilePictureStorage
    {
        string Save(int playerAccountId, byte[] content, string fileExtension);

        byte[] Load(string storedPath);

        void Delete(string storedPath);
    }
}
