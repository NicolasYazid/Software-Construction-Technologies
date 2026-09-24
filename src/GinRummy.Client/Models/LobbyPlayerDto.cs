namespace GinRummy.Client.Models
{
    /// <summary>
    /// Player as the panel of players of the lobby lists them.
    /// </summary>
    public sealed class LobbyPlayerDto
    {
        /// <summary>
        /// Gets or sets the name the player chose, which is never translated.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the name of the rank of the player, as the catalogue of ranks delivers it.
        /// </summary>
        public string RankName { get; set; }
        /// <summary>
        /// Gets or sets whether the player is a friend of whoever looks at the panel, which
        /// decides the actions its context menu offers.
        /// </summary>
        public bool IsFriend { get; set; }
    }
}
