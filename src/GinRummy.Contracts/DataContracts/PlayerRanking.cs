using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    /// <summary>
    /// One row of the leaderboard as the rankings screen (P18) shows it.
    /// </summary>
    [DataContract]
    public class PlayerRanking
    {
        /// <summary>
        /// Place of the player in the leaderboard, starting at one.
        /// </summary>
        [DataMember]
        public int Position { get; set; }

        /// <summary>
        /// Name the player shows to the others.
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Name of the rank the score of the player falls into; empty when no rank contains it.
        /// </summary>
        [DataMember]
        public string RankName { get; set; }

        /// <summary>
        /// Matches the player has won.
        /// </summary>
        [DataMember]
        public int Wins { get; set; }

        /// <summary>
        /// Matches the player has lost.
        /// </summary>
        [DataMember]
        public int Losses { get; set; }

        /// <summary>
        /// Share of matches won, from zero to one.
        /// </summary>
        [DataMember]
        public double WinRate { get; set; }
    }
}
