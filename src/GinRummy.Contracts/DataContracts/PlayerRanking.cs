using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    [DataContract]
    public class PlayerRanking
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
