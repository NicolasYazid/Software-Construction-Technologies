using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    // Public profile of a player (CU-27), or the reduced one of a guest, as the profile screen shows it.
    // The relation to whoever looks at it decides the actions the screen offers.
    public sealed class PlayerProfileDto
    {
        public string Username { get; set; }
        public string PublicTag { get; set; }
        public string RankName { get; set; }
        public bool IsOnline { get; set; }
        // The text the player wrote about themselves is never translated.
        public string Bio { get; set; }
        public IList<SocialLinkDto> SocialLinks { get; set; }
        public int MatchesPlayed { get; set; }
        public double WinRate { get; set; }
        public int Score { get; set; }
        public bool IsGuest { get; set; }
        public bool IsOwnProfile { get; set; }
        public bool IsFriend { get; set; }
        // A request still waiting for an answer keeps a second one from being sent (CU-12).
        public bool HasPendingRequest { get; set; }
    }
}
