using System;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A period during which a registered player is signed in, mapped to the PlayerSession
    /// table. A session is closed by setting its end, never by deleting it, and a player can
    /// have only one open session at a time. Protects its own invariants: it cannot end
    /// before it started or end twice.
    /// </summary>
    public class PlayerSession
    {
        /// <summary>
        /// Creates a new open session.
        /// </summary>
        /// <param name="playerId">Identifier of the player who signed in; must be positive.</param>
        /// <param name="isPersistent">Whether the player asked to stay signed in.</param>
        /// <param name="startedAt">Moment the player signed in, in UTC.</param>
        public PlayerSession(int playerId, bool isPersistent, DateTime startedAt)
        {
            ValidatePlayerId(playerId);

            PlayerId = playerId;
            IsPersistent = isPersistent;
            StartedAt = startedAt;
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected PlayerSession()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int PlayerSessionId { get; private set; }

        /// <summary>
        /// Gets the identifier of the player who signed in.
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        /// Gets whether the player asked to stay signed in.
        /// </summary>
        public bool IsPersistent { get; private set; }

        /// <summary>
        /// Gets the moment the player signed in, in UTC.
        /// </summary>
        public DateTime StartedAt { get; private set; }

        /// <summary>
        /// Gets the moment the session was closed, in UTC. Null while the session is open.
        /// </summary>
        public DateTime? EndedAt { get; private set; }

        /// <summary>
        /// Gets whether the session is still open.
        /// </summary>
        public bool IsActive
        {
            get { return !EndedAt.HasValue; }
        }

        /// <summary>
        /// Closes the session.
        /// </summary>
        /// <param name="endedAt">Moment the session was closed, in UTC; not before it started.</param>
        /// <exception cref="InvalidOperationException">Thrown when the session was already closed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the end is before the start.</exception>
        public void End(DateTime endedAt)
        {
            if (EndedAt.HasValue)
            {
                throw new InvalidOperationException("The session was already ended.");
            }

            if (endedAt < StartedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(endedAt), "A session cannot end before it started.");
            }

            EndedAt = endedAt;
        }

        // A session always belongs to a real player, referenced by a positive id.
        private static void ValidatePlayerId(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentException("Player id must be positive.", nameof(playerId));
            }
        }
    }
}
