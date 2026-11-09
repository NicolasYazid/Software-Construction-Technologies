namespace GinRummy.Domain.Ports.PlayerPreferences
{
    /// <summary>
    /// Contract the game's logic uses to reach PlayerPreferences data, without knowing how or
    /// where it is stored. Infrastructure provides the implementation.
    /// </summary>
    public interface IPlayerPreferencesRepository
    {
        /// <summary>
        /// Finds the preferences of a player by the identifier of its account, or null when they do not exist.
        /// </summary>
        /// <param name="playerAccountId">Identifier of the account that owns the preferences.</param>
        /// <returns>The matching preferences, or null.</returns>
        Entities.PlayerPreferences.PlayerPreferences FindById(int playerAccountId);

        /// <summary>
        /// Adds new preferences and persists them immediately.
        /// </summary>
        /// <param name="newPlayerPreferences">Preferences to create.</param>
        void Add(Entities.PlayerPreferences.PlayerPreferences newPlayerPreferences);
    }
}