using System;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    /// <summary>
    /// A warning a player receives when a report against them is verified, mapped to the
    /// Warning table. Each report produces at most one warning, and the warnings that add up
    /// to a ban are associated with it. Protects its own invariants: it references a real
    /// player and report and it can be associated with only one ban.
    /// </summary>
    public class Warning
    {
        /// <summary>
        /// Creates a valid new warning that is not associated with any ban yet.
        /// </summary>
        /// <param name="playerId">Identifier of the warned player; must be positive.</param>
        /// <param name="reportId">Identifier of the verified report; must be positive.</param>
        /// <param name="issuedAt">Moment the warning was issued, in UTC.</param>
        public Warning(int playerId, int reportId, DateTime issuedAt)
        {
            ValidateId(playerId, nameof(playerId));
            ValidateId(reportId, nameof(reportId));

            PlayerId = playerId;
            ReportId = reportId;
            IssuedAt = issuedAt;
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected Warning()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int WarningId { get; private set; }

        /// <summary>
        /// Gets the identifier of the warned player.
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        /// Gets the identifier of the report that produced the warning.
        /// </summary>
        public int ReportId { get; private set; }

        /// <summary>
        /// Gets the identifier of the ban the warning added up to. Null while the warning has
        /// not led to a ban.
        /// </summary>
        public int? BanId { get; private set; }

        /// <summary>
        /// Gets the moment the warning was issued, in UTC.
        /// </summary>
        public DateTime IssuedAt { get; private set; }

        /// <summary>
        /// Associates the warning with the ban the accumulated warnings led to.
        /// </summary>
        /// <param name="banId">Identifier of the ban; must be positive.</param>
        /// <exception cref="InvalidOperationException">Thrown when the warning already belongs to a ban.</exception>
        public void AssociateWithBan(int banId)
        {
            ValidateId(banId, nameof(banId));

            if (BanId.HasValue)
            {
                throw new InvalidOperationException("The warning is already associated with a ban.");
            }

            BanId = banId;
        }

        // The warning references rows that already exist, so their ids must be positive.
        private static void ValidateId(int id, string parameterName)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be positive.", parameterName);
            }
        }
    }
}
