using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    /// <summary>
    /// Public profile of a player (CU-27), or the reduced one of a guest, as the profile screen
    /// shows it. The relation to whoever looks at it decides the actions the screen offers.
    /// </summary>
    public sealed class PlayerProfileDto
    {
        /// <summary>
        /// Gets or sets the name of the player, or the temporary name of a guest.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the public tag, generated from the identifier of the player.
        /// </summary>
        public string PublicTag { get; set; }
        /// <summary>
        /// Gets or sets the name of the rank, as the catalogue of ranks delivers it.
        /// </summary>
        public string RankName { get; set; }
        /// <summary>
        /// Gets or sets whether the player has an active session.
        /// </summary>
        public bool IsOnline { get; set; }
        /// <summary>
        /// Gets or sets the text the player wrote about themselves, which is never translated.
        /// </summary>
        public string Bio { get; set; }
        /// <summary>
        /// Gets or sets the social networks linked to the profile.
        /// </summary>
        public IList<SocialLinkDto> SocialLinks { get; set; }
        /// <summary>
        /// Gets or sets how many matches the player has finished.
        /// </summary>
        public int MatchesPlayed { get; set; }
        /// <summary>
        /// Gets or sets the share of matches won, from zero to one.
        /// </summary>
        public double WinRate { get; set; }
        /// <summary>
        /// Gets or sets the score that places the player in a rank.
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// Gets or sets whether the profile belongs to a guest, which has none of its own.
        /// </summary>
        public bool IsGuest { get; set; }
        /// <summary>
        /// Gets or sets whether the profile belongs to whoever looks at it.
        /// </summary>
        public bool IsOwnProfile { get; set; }
        /// <summary>
        /// Gets or sets whether the player is a friend of whoever looks at the profile.
        /// </summary>
        public bool IsFriend { get; set; }
        /// <summary>
        /// Gets or sets whether a friend request sent to the player is still waiting for an
        /// answer, which keeps a second one from being sent (CU-12).
        /// </summary>
        public bool HasPendingRequest { get; set; }
    }
}
