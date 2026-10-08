namespace GinRummy.Client.Models
{
    public sealed class MatchPairDto
    {
        public LobbyPlayerDto FirstPlayer { get; set; }
        public LobbyPlayerDto SecondPlayer { get; set; }
    }
}
