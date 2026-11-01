using GinRummy.Domain.Enums;

namespace GinRummy.Application.Ports
{
    /// <summary>
    /// Output port the coordinator uses to push one friend's presence change to a connected
    /// player, without knowing how the change travels. Services implements it over WCF.
    /// </summary>
    public interface IFriendPresenceNotificationPort
    {
        /// <summary>
        /// Pushes to a recipient that one of their friends changed presence.
        /// </summary>
        /// <param name="recipientPlayerId">Internal id of the player who receives the notice.</param>
        /// <param name="changedPlayerId">Internal id of the player whose presence changed.</param>
        /// <param name="state">New presence state of the changed player.</param>
        void NotifyPresenceChanged(int recipientPlayerId, int changedPlayerId, PresenceState state);
    }
}
