using System;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    /// <summary>
    /// A sanction applied to a player, mapped to the Ban table. A ban without an expiry is
    /// permanent. Protects its own invariants: it references a real player and reason and it
    /// cannot expire before it was issued.
    /// </summary>
    public class Ban
    {
        /// <summary>
        /// Creates a valid new ban.
        /// </summary>
        /// <param name="playerId">Identifier of the sanctioned player; must be positive.</param>
        /// <param name="reasonBanId">Identifier of the reason of the sanction; must be positive.</param>
        /// <param name="issuedAt">Moment the ban was issued, in UTC.</param>
        /// <param name="expiresAt">Moment the ban ends, in UTC; null for a permanent ban.</param>
        public Ban(int playerId, int reasonBanId, DateTime issuedAt, DateTime? expiresAt)
        {
            ValidateId(playerId, nameof(playerId));
            ValidateId(reasonBanId, nameof(reasonBanId));
            ValidateExpiry(issuedAt, expiresAt);

            PlayerId = playerId;
            ReasonBanId = reasonBanId;
            IssuedAt = issuedAt;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected Ban()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int BanId { get; private set; }

        /// <summary>
        /// Gets the identifier of the sanctioned player.
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        /// Gets the identifier of the reason of the sanction.
        /// </summary>
        public int ReasonBanId { get; private set; }

        /// <summary>
        /// Gets the moment the ban was issued, in UTC.
        /// </summary>
        public DateTime IssuedAt { get; private set; }

        /// <summary>
        /// Gets the moment the ban ends, in UTC. Null when the ban is permanent.
        /// </summary>
        public DateTime? ExpiresAt { get; private set; }

        /// <summary>
        /// Gets the reason of the sanction, loaded from the relationship between the two
        /// tables.
        /// </summary>
        public virtual ReasonBan ReasonBan { get; private set; }

        /// <summary>
        /// Gets whether the ban never ends.
        /// </summary>
        public bool IsPermanent
        {
            get { return !ExpiresAt.HasValue; }
        }

        /// <summary>
        /// Tells whether the ban is in force at the given moment.
        /// </summary>
        /// <param name="moment">Moment to check, in UTC; normally the server clock.</param>
        /// <returns>True when the ban is permanent or has not expired yet.</returns>
        public bool IsActiveAt(DateTime moment)
        {
            bool isActive = IsPermanent || (ExpiresAt.Value > moment);

            return isActive;
        }

        // The ban references rows that already exist, so their ids must be positive.
        private static void ValidateId(int id, string parameterName)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be positive.", parameterName);
            }
        }

        // A temporary ban that ends at or before its start would be born already over.
        private static void ValidateExpiry(DateTime issuedAt, DateTime? expiresAt)
        {
            if (expiresAt.HasValue && (expiresAt.Value <= issuedAt))
            {
                throw new ArgumentException("Expiry must be after the issue moment.", nameof(expiresAt));
            }
        }
    }
}
