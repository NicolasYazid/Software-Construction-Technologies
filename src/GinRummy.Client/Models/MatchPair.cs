using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Models
{
    public sealed class MatchPair
    {
        public LobbyPlayerViewModel FirstPlayer { get; set; }
        public LobbyPlayerViewModel SecondPlayer { get; set; }
    }
}
