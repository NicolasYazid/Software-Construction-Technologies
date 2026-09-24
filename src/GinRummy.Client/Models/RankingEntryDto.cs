namespace GinRummy.Client.Models
{
    /// <summary>
    /// Row of the leaderboard (CU-19): the place of a player and the performance that earns it.
    /// </summary>
    public sealed class RankingEntryDto
    {
        /// <summary>
        /// Gets or sets the place of the player, counted from one.
        /// </summary>
        public int Position { get; set; }
        /// <summary>
        /// Gets or sets the name the player chose, which is never translated.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the name of the rank of the player, as the catalogue of ranks delivers it.
        /// </summary>
        public string RankName { get; set; }
        /// <summary>
        /// Gets or sets how many matches the player has won.
        /// </summary>
        public int Wins { get; set; }
        /// <summary>
        /// Gets or sets how many matches the player has lost.
        /// </summary>
        public int Losses { get; set; }
        /// <summary>
        /// Gets or sets the share of matches won, from zero to one.
        /// </summary>
        public double WinRate { get; set; }
        /// <summary>
        /// Gets or sets whether the row belongs to whoever looks at the leaderboard, which the
        /// table highlights.
        /// </summary>
        public bool IsOwnEntry { get; set; }
    }
}
