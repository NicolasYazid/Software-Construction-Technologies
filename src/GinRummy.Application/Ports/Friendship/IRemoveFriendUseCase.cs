namespace GinRummy.Application.Ports.Friendship
{
    public interface IRemoveFriendUseCase
    {
        void RemoveFriend(int playerId, int friendPublicTag);
    }
}
