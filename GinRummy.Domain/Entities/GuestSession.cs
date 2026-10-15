using System;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A visit by someone who plays without an account, mapped to the GuestSession table. The
    /// guest has no credentials: only a temporary name that lasts as long as the visit.
    /// Protects its own invariants: the name is required and fits the column.
    /// </summary>
    public class GuestSession
    {
        private const int MaxDisplayNameLength = 20;

        /// <summary>
        /// Creates a valid new guest session.
        /// </summary>
        /// <param name="temporalDisplayName">Temporary name; required, at most 20 characters.</param>
        /// <param name="createdAt">Moment the guest arrived, in UTC.</param>
        public GuestSession(string temporalDisplayName, DateTime createdAt)
        {
            ValidateDisplayName(temporalDisplayName);

            TemporalDisplayName = temporalDisplayName.Trim();
            CreatedAt = createdAt;
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected GuestSession()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int GuestSessionId { get; private set; }

        /// <summary>
        /// Gets the temporary name shown to the other players.
        /// </summary>
        public string TemporalDisplayName { get; private set; }

        /// <summary>
        /// Gets the moment the guest arrived, in UTC.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        // A guest is always shown with a name, and the name must fit the column.
        private static void ValidateDisplayName(string temporalDisplayName)
        {
            if (string.IsNullOrWhiteSpace(temporalDisplayName))
            {
                throw new ArgumentException("Display name is required.", nameof(temporalDisplayName));
            }

            if (temporalDisplayName.Trim().Length > MaxDisplayNameLength)
            {
                throw new ArgumentException("Display name is too long.", nameof(temporalDisplayName));
            }
        }
    }
}
