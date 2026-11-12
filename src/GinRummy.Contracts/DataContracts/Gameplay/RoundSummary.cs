using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.Gameplay
{
    [DataContract]
    public class RoundSummary
    {
        [DataMember]
        public RoundOutcome Ending { get; set; }

        [DataMember]
        public string WinnerName { get; set; }

        [DataMember]
        public bool IsWinner { get; set; }

        [DataMember]
        public int Points { get; set; }

        [DataMember]
        public string KnockerName { get; set; }

        [DataMember]
        public int KnockerDeadwood { get; set; }

        [DataMember]
        public int DefenderDeadwood { get; set; }

        [DataMember]
        public List<List<PlayingCard>> KnockerMelds { get; set; }

        [DataMember]
        public List<PlayingCard> KnockerUnmatchedCards { get; set; }

        [DataMember]
        public List<List<PlayingCard>> DefenderMelds { get; set; }

        [DataMember]
        public List<PlayingCard> DefenderUnmatchedCards { get; set; }
    }
}
