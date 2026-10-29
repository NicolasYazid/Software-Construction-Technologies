using System.Runtime.Serialization;

namespace GinRummy.Contracts
{
    /// <summary>
    /// A single leaderboard row as the rankings screen (P18) shows it.
    /// </summary>
    [DataContract]
    public class PlayerRankingDto
    {
        [DataMember]
        public int Position { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string RankName { get; set; }
        [DataMember]
        public int Wins { get; set; }
        [DataMember]
        public int Losses { get; set; }
        [DataMember]
        public double WinRate { get; set; }
    }
}
