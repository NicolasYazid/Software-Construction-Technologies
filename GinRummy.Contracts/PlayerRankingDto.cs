using System.Runtime.Serialization;

namespace GinRummy.Contracts
{
    /// <summary>
    /// A single leaderboard row as the rankings screen (P18) shows it.
    /// </summary>
    [DataContract]
    public class PlayerRankingDto
    {
        /// <summary>
        /// Gets or sets the place the player holds on the leaderboard, starting at one.
        /// </summary>
        [DataMember]
        public int Position { get; set; }
        /// <summary>
        /// Gets or sets the username of the player in this row.
        /// </summary>
        [DataMember]
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the name of the rank the player's score falls into; empty when no rank matches.
        /// </summary>
        [DataMember]
        public string RankName { get; set; }
        /// <summary>
        /// Gets or sets the number of matches the player has won.
        /// </summary>
        [DataMember]
        public int Wins { get; set; }
        /// <summary>
        /// Gets or sets the number of matches the player has lost.
        /// </summary>
        [DataMember]
        public int Losses { get; set; }
        /// <summary>
        /// Gets or sets the ratio of wins to matches played; zero when no match has been played.
        /// </summary>
        [DataMember]
        public double WinRate { get; set; }
    }
}
