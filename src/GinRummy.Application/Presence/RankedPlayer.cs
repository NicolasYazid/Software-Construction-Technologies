using System;

using GinRummy.Domain.Entities.PlayerRanking;
using GinRummy.Domain.Entities.Players;

namespace GinRummy.Application.Presence
{
    // The lists of the lobby go from the highest rank to the lowest, so every player travels with its score and its rank.
    public class RankedPlayer
    {
        public RankedPlayer(Player player, int score, Rank rank)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Score = score;
            Rank = rank;
        }

        public Player Player { get; private set; }

        public int Score { get; private set; }

        // No rank contains the score when the catalogue leaves a gap, and the player is still listed.
        public Rank Rank { get; private set; }
    }
}
