using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    public sealed class PlayerProfile
    {
        // The database limits the biography to five hundred characters.
        public const int BioMaxLength = 500;

        public string Username { get; set; }
        public string PublicTag { get; set; }
        public string RankName { get; set; }
        public string Bio { get; set; }
        public IList<SocialLink> SocialLinks { get; set; }
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
