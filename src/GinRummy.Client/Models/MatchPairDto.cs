namespace GinRummy.Client.Models
{
    // Two players who are playing a match against each other, listed together in the panel of players of the lobby.
    public sealed class MatchPairDto
    {
        public LobbyPlayerDto FirstPlayer { get; set; }
        public LobbyPlayerDto SecondPlayer { get; set; }
    }
}
