namespace GinRummy.Client.Models
{
    /// <summary>
    /// Rank of a card of the French deck. The value of each member is the number the card
    /// shows, from the ace, which is worth one, to the king.
    /// </summary>
    public enum CardRank
    {
        /// <summary>
        /// The ace, worth one and always at the start of a run.
        /// </summary>
        Ace = 1,

        /// <summary>
        /// The two.
        /// </summary>
        Two = 2,

        /// <summary>
        /// The three.
        /// </summary>
        Three = 3,

        /// <summary>
        /// The four.
        /// </summary>
        Four = 4,

        /// <summary>
        /// The five.
        /// </summary>
        Five = 5,

        /// <summary>
        /// The six.
        /// </summary>
        Six = 6,

        /// <summary>
        /// The seven.
        /// </summary>
        Seven = 7,

        /// <summary>
        /// The eight.
        /// </summary>
        Eight = 8,

        /// <summary>
        /// The nine.
        /// </summary>
        Nine = 9,

        /// <summary>
        /// The ten.
        /// </summary>
        Ten = 10,

        /// <summary>
        /// The jack, a face card.
        /// </summary>
        Jack = 11,

        /// <summary>
        /// The queen, a face card.
        /// </summary>
        Queen = 12,

        /// <summary>
        /// The king, a face card.
        /// </summary>
        King = 13
    }
}
