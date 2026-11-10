using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.Gameplay
{
    [DataContract]
    public class RoundDetails
    {
        [DataMember]
        public string OpponentName { get; set; }

        [DataMember]
        public bool IsYourTurn { get; set; }

        [DataMember]
        public RoundStep Step { get; set; }

        [DataMember]
        public List<PlayingCard> Hand { get; set; }

        [DataMember]
        public int OpponentCardCount { get; set; }

        [DataMember]
        public int StockCount { get; set; }

        [DataMember]
        public PlayingCard TopOfDiscardPile { get; set; }

        [DataMember]
        public int Deadwood { get; set; }

        [DataMember]
        public RoundMoveKind LastMove { get; set; }

        [DataMember]
        public string LastMoverName { get; set; }

        [DataMember]
        public PlayingCard LastMoveCard { get; set; }
    }
}
