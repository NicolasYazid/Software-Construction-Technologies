namespace GinRummy.Client.Models
{
    // Values of the configuration of the game that the house rules quote (CU-21 step 3).
    // They are kept out of the translated text so that changing one never forces a new translation.
    public sealed class GameRulesDto
    {
        public int CardsPerHand { get; set; }
        public int KnockThreshold { get; set; }
        public int GinBonus { get; set; }
        public int UndercutBonus { get; set; }
        public int StockCardsToVoid { get; set; }
        public int TargetScore { get; set; }
    }
}
