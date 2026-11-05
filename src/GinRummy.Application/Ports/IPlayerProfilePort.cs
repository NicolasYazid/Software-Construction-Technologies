using GinRummy.Application.UseCases;

namespace GinRummy.Application.Ports
{
    public interface IPlayerProfilePort
    {
        ViewedPlayerProfile GetPlayerProfile(string accessToken, int publicTag);
    }
}
