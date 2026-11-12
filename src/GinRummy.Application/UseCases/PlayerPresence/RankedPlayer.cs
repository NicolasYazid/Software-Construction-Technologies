using System;

using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCustomization;

namespace GinRummy.Application.UseCases.PlayerPresence
{
    public class RankedPlayer
    {
        public RankedPlayer(PlayerProfile player, int score, Rank rank)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            AccessTokenOwner = AccessTokenOwner.OfPlayerAccount(player.PlayerAccountId);
            DisplayName = player.Username;
            PublicTag = player.PublicTag;
            Score = score;
            Rank = rank;
        }

        public RankedPlayer(GuestSession guest)
        {
            if (guest == null)
            {
                throw new ArgumentNullException(nameof(guest));
            }

            AccessTokenOwner = AccessTokenOwner.OfGuestSession(guest.GuestSessionId);
            DisplayName = guest.TemporalDisplayName;
        }

        public AccessTokenOwner AccessTokenOwner { get; private set; }

        public string DisplayName { get; private set; }

        public int? PublicTag { get; private set; }

        public bool IsGuest
        {
            get { return AccessTokenOwner.IsGuest; }
        }

        public int Score { get; private set; }

        public Rank Rank { get; private set; }
    }
}
