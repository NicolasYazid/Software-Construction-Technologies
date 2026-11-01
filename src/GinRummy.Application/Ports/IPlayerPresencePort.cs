using System.Collections.Generic;

using GinRummy.Domain.Enums;

namespace GinRummy.Application.Ports
{
    /// <summary>
    /// Input port that drives the presence feature: a player subscribes to the presence of their
    /// friends and later unsubscribes. The coordinator keeps the connected players in memory.
    /// </summary>
    public interface IPlayerPresencePort
    {
        /// <summary>
        /// Marks the player as connected and returns the current state of each of their friends.
        /// </summary>
        /// <param name="playerId">Internal id of the player that connects.</param>
        /// <returns>Each friend's internal id paired with their state; empty when the player has no friends.</returns>
        IReadOnlyDictionary<int, PresenceState> SubscribeToFriendsPresence(int playerId);

        /// <summary>
        /// Marks the player as disconnected and pushes the change to their connected friends.
        /// </summary>
        /// <param name="playerId">Internal id of the player that disconnects.</param>
        void UnsubscribeFromFriendsPresence(int playerId);
    }
}
