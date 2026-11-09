using GinRummy.Application.UseCases.PlayerCustomization;

namespace GinRummy.Application.Ports.PlayerCustomization
{
    public interface IPlayerProfilePort
    {
        ViewedPlayerProfile GetPlayerProfile(string accessToken, int publicTag);
    }
}
