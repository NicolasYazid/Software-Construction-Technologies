using System;

using GinRummy.Domain.Entities.PlayerRanking;
using GinRummy.Domain.Entities.Players;

namespace GinRummy.Application.UseCases
{
    public class ViewedPlayerProfile
    {
        public ViewedPlayerProfile(PlayerProfile profile, PlayerStats stats, Rank rank)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            Rank = rank;
        }

        public PlayerProfile Profile { get; private set; }

        public PlayerStats Stats { get; private set; }

        // No rank contains the score when the catalogue leaves a gap, and the profile is still shown (CU-27 EX-05).
        public Rank Rank { get; private set; }
    }
}
