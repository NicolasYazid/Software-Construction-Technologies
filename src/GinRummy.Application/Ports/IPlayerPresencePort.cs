namespace GinRummy.Application.Ports
{
    public interface IPlayerPresencePort
    {
        int SubscribeToPlayerPresence(string accessToken);

        void ReportCurrentStatusOfPlayers(int playerId);

        void UnsubscribeFromPlayerPresence(int playerId);
    }
}
