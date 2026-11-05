using System;

using GinRummy.Domain.Entities.PlayerRanking;
using GinRummy.Domain.Entities.Players;

namespace GinRummy.Application.Presence
{
    // The lists of the lobby go from the highest rank to the lowest, so every player travels with its score and its rank.
    // A guest is listed too, without score or rank, because the lobby shows everyone who is connected (CU-27 FA-04).
    public class RankedPlayer
    {
        public RankedPlayer(Player player, int score, Rank rank)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            IdentityId = player.PlayerId;
            DisplayName = player.Username;
            PublicTag = player.PublicTag;
            Score = score;
            Rank = rank;
        }

        // A guest has no public tag, so its identity tells its row apart without matching any tag a player types.
        public RankedPlayer(GuestSession guest)
        {
            if (guest == null)
            {
                throw new ArgumentNullException(nameof(guest));
            }

            IdentityId = guest.IdentityId;
            DisplayName = guest.TemporalDisplayName;
            PublicTag = guest.IdentityId;
            IsGuest = true;
        }

        public int IdentityId { get; private set; }

        public string DisplayName { get; private set; }

        public int PublicTag { get; private set; }

        public bool IsGuest { get; private set; }

        public int Score { get; private set; }

        // No rank contains the score when the catalogue leaves a gap, and the player is still listed.
        public Rank Rank { get; private set; }
    }
}
