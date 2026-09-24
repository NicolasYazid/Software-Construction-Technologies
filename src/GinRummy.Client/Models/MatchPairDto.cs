namespace GinRummy.Client.Models
{
    /// <summary>
    /// Two players who are playing a match against each other, listed together in the panel
    /// of players of the lobby.
    /// </summary>
    public sealed class MatchPairDto
    {
        /// <summary>
        /// Gets or sets the player drawn on the left of the pair.
        /// </summary>
        public LobbyPlayerDto FirstPlayer { get; set; }
        /// <summary>
        /// Gets or sets the player drawn on the right of the pair.
        /// </summary>
        public LobbyPlayerDto SecondPlayer { get; set; }
    }
}
