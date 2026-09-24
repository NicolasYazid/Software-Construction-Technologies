namespace GinRummy.Client.Models
{
    /// <summary>
    /// Values of the configuration of the game that the house rules quote (CU-21 step 3). They
    /// are kept out of the translated text so that changing one never forces a new translation.
    /// </summary>
    public sealed class GameRulesDto
    {
        /// <summary>
        /// Gets or sets how many cards each player receives.
        /// </summary>
        public int CardsPerHand { get; set; }
        /// <summary>
        /// Gets or sets the highest deadwood with which a player may knock.
        /// </summary>
        public int KnockThreshold { get; set; }
        /// <summary>
        /// Gets or sets the bonus of knocking with no unmatched cards.
        /// </summary>
        public int GinBonus { get; set; }
        /// <summary>
        /// Gets or sets the bonus of the player who ends with equal or less deadwood than the
        /// one who knocked.
        /// </summary>
        public int UndercutBonus { get; set; }
        /// <summary>
        /// Gets or sets how many cards left in the stock void the hand.
        /// </summary>
        public int StockCardsToVoid { get; set; }
        /// <summary>
        /// Gets or sets the score that wins the match.
        /// </summary>
        public int TargetScore { get; set; }
    }
}
