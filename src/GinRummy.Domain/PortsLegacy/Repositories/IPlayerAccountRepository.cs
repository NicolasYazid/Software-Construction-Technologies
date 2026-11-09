using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Entities.Players;

namespace GinRummy.Domain.Ports.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach PlayerAccount data, without knowing how or
    /// where it is stored. Infrastructure provides the implementation.
    /// </summary>
    public interface IPlayerAccountRepository
    {
        /// <summary>
        /// Finds the account whose email matches, with its player and the player's language loaded,
        /// or null when no account has it.
        /// </summary>
        /// <param name="email">Email address to search for.</param>
        /// <returns>The matching account, or null.</returns>
        PlayerAccount FindByEmail(string email);

        /// <summary>
        /// Finds the account of a player, with its player loaded, or null when the player has
        /// no account.
        /// </summary>
        /// <param name="playerAccountId">Identifier of the player that owns the account.</param>
        /// <returns>The matching account, or null.</returns>
        PlayerAccount FindByPlayerAccountId(int playerAccountId);

        PlayerProfile FindProfileByPlayerAccountId(int playerAccountId);

        /// <summary>
        /// Adds a new account and persists it immediately.
        /// </summary>
        /// <param name="newAccount">Account to create.</param>
        /// <exception cref="EmailAlreadyRegisteredException">Thrown when another account already has the email.</exception>
        void Add(PlayerAccount newAccount);

        /// <summary>
        /// Adds the profile of a new account and persists it immediately.
        /// </summary>
        /// <param name="newProfile">Profile to create.</param>
        void AddProfile(PlayerProfile newProfile);

        /// <summary>
        /// Adds the statistics of a new account and persists them immediately.
        /// </summary>
        /// <param name="newStats">Statistics to create.</param>
        void AddStats(PlayerStats newStats);

        /// <summary>
        /// Finds the profile of a player, with its social links loaded, or null when the player has no
        /// profile.
        /// </summary>
        /// <param name="playerAccountId">Identifier of the player that owns the profile.</param>
        /// <returns>The matching profile, or null.</returns>
        PlayerProfile FindProfile(int playerAccountId);

        /// <summary>
        /// Finds a player by the public tag other players see, or null when no player has it.
        /// </summary>
        /// <param name="publicTag">Public tag to search for.</param>
        /// <returns>The matching profile, or null.</returns>
        PlayerProfile FindByPublicTag(int publicTag);

        /// <summary>
        /// Gets every registered player.
        /// </summary>
        /// <returns>All the profiles; empty when none has registered.</returns>
        IList<PlayerProfile> GetAllPlayers();

        /// <summary>
        /// Finds the statistics of a player, with its player loaded, or null when the player has none.
        /// </summary>
        /// <param name="playerAccountId">Identifier of the player that owns the statistics.</param>
        /// <returns>The matching statistics, or null.</returns>
        PlayerStats FindStats(int playerAccountId);

        /// <summary>
        /// Adds a new social media link and persists it immediately, so its identifier is known right
        /// after the call.
        /// </summary>
        /// <param name="newLink">Link to create.</param>
        void AddSocialMedia(SocialMedia newLink);

        /// <summary>
        /// Deletes a social media link and persists the change immediately.
        /// </summary>
        /// <param name="link">Link to delete, read through the same repository.</param>
        void RemoveSocialMedia(SocialMedia link);
    }
}
