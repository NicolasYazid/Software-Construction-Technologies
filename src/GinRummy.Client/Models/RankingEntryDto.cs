namespace GinRummy.Client.Models
{
    // Row of the leaderboard (CU-19): the place of a player and the performance that earns it.
    public sealed class RankingEntryDto
    {
        public int Position { get; set; }
        // The name the player chose is never translated.
        public string Username { get; set; }
        public string RankName { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinRate { get; set; }
        public bool IsOwnEntry { get; set; }
    }
}
