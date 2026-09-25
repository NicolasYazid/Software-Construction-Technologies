namespace GinRummy.Client.Models
{
    /// <summary>
    /// Kind of group a hand is laid down in when it closes.
    /// </summary>
    public enum MeldKind
    {
        /// <summary>
        /// Three or four cards of the same rank.
        /// </summary>
        Set,

        /// <summary>
        /// Three or more consecutive cards of the same suit.
        /// </summary>
        Run,

        /// <summary>
        /// The cards that form no group, whose sum is the deadwood.
        /// </summary>
        Unmatched
    }
}
