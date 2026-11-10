using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts.Gameplay
{
    [DataContract]
    public class PlayingCard
    {
        [DataMember]
        public CardSuit Suit { get; set; }

        [DataMember]
        public int Rank { get; set; }
    }
}
