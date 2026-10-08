namespace GinRummy.Client.Models
{
    public sealed class RankingEntryDto
    {
        public int Position { get; set; }
        public string Username { get; set; }
        public string RankName { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinRate { get; set; }
        public bool IsOwnEntry { get; set; }
    }
}
