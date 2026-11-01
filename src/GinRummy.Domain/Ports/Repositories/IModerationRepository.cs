using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerModeration;

namespace GinRummy.Domain.Ports.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach warnings and bans together with their
    /// catalogs, without knowing how or where they are stored.
    /// </summary>
    public interface IModerationRepository
    {
        /// <summary>
        /// Finds a sanction reason by its identifier, or null when it does not exist.
        /// </summary>
        /// <param name="reasonBanId">Identifier of the reason.</param>
        /// <returns>The matching reason, or null.</returns>
        ReasonBan FindReasonBanById(int reasonBanId);

        /// <summary>
        /// Adds a new warning and persists it immediately.
        /// </summary>
        /// <param name="newWarning">Warning to create.</param>
        void AddWarning(Warning newWarning);

        /// <summary>
        /// Adds a new ban and persists it immediately, so its identifier is known right
        /// after the call.
        /// </summary>
        /// <param name="newBan">Ban to create.</param>
        void AddBan(Ban newBan);

        /// <summary>
        /// Links a warning to the ban it ended up causing and persists the change.
        /// </summary>
        /// <param name="warningId">Warning to update.</param>
        /// <param name="banId">Ban the warning led to.</param>
        void AssociateWarningWithBan(int warningId, int banId);

        /// <summary>
        /// Gets every ban a player has received, active or not, each with its reason loaded.
        /// </summary>
        /// <param name="playerId">Player whose bans are wanted.</param>
        /// <returns>The bans, most recent first.</returns>
        IList<Ban> GetBansOf(int playerId);

        /// <summary>
        /// Gets every warning a player has received.
        /// </summary>
        /// <param name="playerId">Player whose warnings are wanted.</param>
        /// <returns>The warnings, most recent first.</returns>
        IList<Warning> GetWarningsOf(int playerId);
    }
}
