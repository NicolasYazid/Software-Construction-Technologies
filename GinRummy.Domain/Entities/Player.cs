using System;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A registered player account, mapped to the Player table. Carries the data the
    /// registration and sign-in flows read and write; it does not enforce game rules.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int PlayerId { get; private set; }
        /// <summary>
        /// Gets the public identifier shown to other players, always PlayerId + 100000.
        /// Computed by SQL Server; never assigned by the application.
        /// </summary>
        public int PublicTag { get; private set; }
        /// <summary>
        /// Gets or sets the display name chosen at registration. Not unique.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the account email address. Unique across all players.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the Argon2id hash of the password. Never the plain text password.
        /// </summary>
        public string PasswordHash { get; set; }
        /// <summary>
        /// Gets or sets the locale configured by the player.
        /// </summary>
        public int LocaleId { get; set; }
        /// <summary>
        /// Gets or sets the registration date, in UTC.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Gets or sets the last sign-in date, in UTC. Null until the first sign-in.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
        /// <summary>
        /// Gets or sets whether the email address has already been verified.
        /// </summary>
        public bool IsEmailVerified { get; set; }
    }
}
