using System;

using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCustomization;

namespace GinRummy.Application.UseCases.PlayerCustomization
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

        public Rank Rank { get; private set; }
    }
}
