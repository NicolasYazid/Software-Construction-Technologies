using System;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Models
{
    public sealed class MatchPair
    {
        public MatchPair(LobbyPlayerViewModel firstPlayer, LobbyPlayerViewModel secondPlayer)
        {
            FirstPlayer = firstPlayer ?? throw new ArgumentNullException(nameof(firstPlayer));
            SecondPlayer = secondPlayer ?? throw new ArgumentNullException(nameof(secondPlayer));
        }

        public LobbyPlayerViewModel FirstPlayer { get; private set; }
        public LobbyPlayerViewModel SecondPlayer { get; private set; }
    }
}
