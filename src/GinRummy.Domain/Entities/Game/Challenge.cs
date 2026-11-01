using System;

using GinRummy.Domain.Entities.Players;

namespace GinRummy.Domain.Entities.Game
{
    /// <summary>
    /// A direct invitation to play between two registered players, mapped to the Challenge
    /// table. Protects its own invariants: a player cannot challenge themselves, the challenge
    /// expires after it was created, and it leaves the Sent status only once, together with
    /// the moment it was resolved; only an accepted challenge points to a match.
    /// </summary>
    public class Challenge
    {
        /// <summary>
        /// Length of the status column.
        /// </summary>
        public const int StatusNameMaxLength = 20;

        /// <summary>
        /// Creates a valid new challenge in the Sent status.
        /// </summary>
        /// <param name="challengerPlayerId">Identifier of the player who invites; must be positive.</param>
        /// <param name="challengedPlayerId">Identifier of the invited player; positive and different.</param>
        /// <param name="createdAt">Moment the challenge was sent, in UTC.</param>
        /// <param name="expiresAt">Moment the challenge stops being valid, in UTC; after createdAt.</param>
        public Challenge(int challengerPlayerId, int challengedPlayerId, DateTime createdAt, DateTime expiresAt)
        {
            ValidatePlayers(challengerPlayerId, challengedPlayerId);
            ValidateExpiration(createdAt, expiresAt);

            ChallengerPlayerId = challengerPlayerId;
            ChallengedPlayerId = challengedPlayerId;
            StatusName = ChallengeStatus.Sent.ToString();
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected Challenge()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int ChallengeId { get; private set; }

        /// <summary>
        /// Gets the identifier of the player who sent the challenge.
        /// </summary>
        public int ChallengerPlayerId { get; private set; }

        /// <summary>
        /// Gets the identifier of the player who received the challenge.
        /// </summary>
        public int ChallengedPlayerId { get; private set; }

        /// <summary>
        /// Gets the status exactly as the status column stores it.
        /// </summary>
        public string StatusName { get; private set; }

        /// <summary>
        /// Gets the moment the challenge was sent, in UTC.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the moment the challenge stops being valid, in UTC.
        /// </summary>
        public DateTime ExpiresAt { get; private set; }

        /// <summary>
        /// Gets the moment the challenge left the Sent status, in UTC. Null while it is pending.
        /// </summary>
        public DateTime? ResolvedAt { get; private set; }

        /// <summary>
        /// Gets the identifier of the match that came from the challenge. Null unless accepted.
        /// </summary>
        public int? GameSessionId { get; private set; }

        /// <summary>
        /// Gets the account of the player who sent the challenge.
        /// </summary>
        public virtual PlayerAccount Challenger { get; private set; }

        /// <summary>
        /// Gets the account of the player who received the challenge.
        /// </summary>
        public virtual PlayerAccount Challenged { get; private set; }

        /// <summary>
        /// Gets the status of the challenge.
        /// </summary>
        public ChallengeStatus Status
        {
            get { return (ChallengeStatus)Enum.Parse(typeof(ChallengeStatus), StatusName); }
        }

        /// <summary>
        /// Gets a value indicating whether the challenge still waits for an answer.
        /// </summary>
        public bool IsPending
        {
            get { return Status == ChallengeStatus.Sent; }
        }

        /// <summary>
        /// Tells whether the challenge is pending and its time is over at a given moment.
        /// </summary>
        /// <param name="moment">Moment to compare against, in UTC.</param>
        /// <returns>True when the challenge should be marked as expired.</returns>
        public bool IsExpiredAt(DateTime moment)
        {
            return IsPending && (moment >= ExpiresAt);
        }

        /// <summary>
        /// Accepts the challenge and links it with the match that comes from it.
        /// </summary>
        /// <param name="gameSessionId">Identifier of the new match; must be positive.</param>
        /// <param name="resolvedAt">Moment of the answer, in UTC; not before the challenge was sent.</param>
        /// <exception cref="InvalidOperationException">Thrown when the challenge is not pending.</exception>
        public void Accept(int gameSessionId, DateTime resolvedAt)
        {
            if (gameSessionId <= 0)
            {
                throw new ArgumentException("Game session id must be positive.", nameof(gameSessionId));
            }

            Resolve(ChallengeStatus.Accepted, resolvedAt);
            GameSessionId = gameSessionId;
        }

        /// <summary>
        /// Declines the challenge.
        /// </summary>
        /// <param name="resolvedAt">Moment of the answer, in UTC; not before the challenge was sent.</param>
        /// <exception cref="InvalidOperationException">Thrown when the challenge is not pending.</exception>
        public void Decline(DateTime resolvedAt)
        {
            Resolve(ChallengeStatus.Declined, resolvedAt);
        }

        /// <summary>
        /// Withdraws the challenge by the player who sent it.
        /// </summary>
        /// <param name="resolvedAt">Moment of the withdrawal, in UTC; not before the challenge was sent.</param>
        /// <exception cref="InvalidOperationException">Thrown when the challenge is not pending.</exception>
        public void Cancel(DateTime resolvedAt)
        {
            Resolve(ChallengeStatus.Cancelled, resolvedAt);
        }

        /// <summary>
        /// Marks the challenge as expired because nobody answered in time.
        /// </summary>
        /// <param name="resolvedAt">Moment the expiration was noticed, in UTC; not before it was sent.</param>
        /// <exception cref="InvalidOperationException">Thrown when the challenge is not pending.</exception>
        public void Expire(DateTime resolvedAt)
        {
            Resolve(ChallengeStatus.Expired, resolvedAt);
        }

        // The database accepts one single leave from Sent, so every answer shares this guard.
        private void Resolve(ChallengeStatus newStatus, DateTime resolvedAt)
        {
            if (!IsPending)
            {
                throw new InvalidOperationException("Only a pending challenge can be resolved.");
            }

            if (resolvedAt < CreatedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(resolvedAt), "A challenge cannot be resolved before it was sent.");
            }

            StatusName = newStatus.ToString();
            ResolvedAt = resolvedAt;
        }

        // Both ends must be real players, and a challenge against oneself has no opponent.
        private static void ValidatePlayers(int challengerPlayerId, int challengedPlayerId)
        {
            if ((challengerPlayerId <= 0) || (challengedPlayerId <= 0))
            {
                throw new ArgumentException("Player ids must be positive.");
            }

            if (challengerPlayerId == challengedPlayerId)
            {
                throw new ArgumentException("A player cannot challenge themselves.", nameof(challengedPlayerId));
            }
        }

        // An invitation that is already expired when it is sent could never be answered.
        private static void ValidateExpiration(DateTime createdAt, DateTime expiresAt)
        {
            if (expiresAt <= createdAt)
            {
                throw new ArgumentException("A challenge must expire after it was created.", nameof(expiresAt));
            }
        }
    }
}
