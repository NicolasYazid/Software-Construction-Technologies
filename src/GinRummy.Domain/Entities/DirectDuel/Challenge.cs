using System;

using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Domain.Entities.DirectDuel
{
    public class Challenge
    {
        public const int StatusNameMaxLength = 20;

        public Challenge(int challengerPlayerAccountId, int challengedPlayerAccountId, DateTime createdAt, DateTime expiresAt)
        {
            ValidatePlayers(challengerPlayerAccountId, challengedPlayerAccountId);
            ValidateExpiration(createdAt, expiresAt);

            ChallengerPlayerAccountId = challengerPlayerAccountId;
            ChallengedPlayerAccountId = challengedPlayerAccountId;
            StatusName = ChallengeStatus.Sent.ToString();
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
        }

        protected Challenge()
        {
        }

        public int ChallengeId { get; private set; }

        public int ChallengerPlayerAccountId { get; private set; }

        public int ChallengedPlayerAccountId { get; private set; }

        public string StatusName { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public DateTime? ResolvedAt { get; private set; }

        public int? GameSessionId { get; private set; }

        public virtual PlayerAccount Challenger { get; private set; }

        public virtual PlayerAccount Challenged { get; private set; }

        public ChallengeStatus Status
        {
            get { return (ChallengeStatus)Enum.Parse(typeof(ChallengeStatus), StatusName); }
        }

        public bool IsPending
        {
            get { return Status == ChallengeStatus.Sent; }
        }

        public bool IsExpiredAt(DateTime moment)
        {
            return IsPending && (moment >= ExpiresAt);
        }

        public void Accept(int gameSessionId, DateTime resolvedAt)
        {
            if (gameSessionId <= 0)
            {
                throw new ArgumentException("Game session id must be positive.", nameof(gameSessionId));
            }

            Resolve(ChallengeStatus.Accepted, resolvedAt);
            GameSessionId = gameSessionId;
        }

        public void Decline(DateTime resolvedAt)
        {
            Resolve(ChallengeStatus.Declined, resolvedAt);
        }

        public void Cancel(DateTime resolvedAt)
        {
            Resolve(ChallengeStatus.Cancelled, resolvedAt);
        }

        public void Expire(DateTime resolvedAt)
        {
            Resolve(ChallengeStatus.Expired, resolvedAt);
        }

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

        private static void ValidatePlayers(int challengerPlayerAccountId, int challengedPlayerAccountId)
        {
            if ((challengerPlayerAccountId <= 0) || (challengedPlayerAccountId <= 0))
            {
                throw new ArgumentException("Player account ids must be positive.");
            }

            if (challengerPlayerAccountId == challengedPlayerAccountId)
            {
                throw new ArgumentException("A player cannot challenge themselves.", nameof(challengedPlayerAccountId));
            }
        }

        private static void ValidateExpiration(DateTime createdAt, DateTime expiresAt)
        {
            if (expiresAt <= createdAt)
            {
                throw new ArgumentException("A challenge must expire after it was created.", nameof(expiresAt));
            }
        }
    }
}
