using System;

namespace GinRummy.Domain.Ports.Repositories
{
    /// <summary>
    /// A set of repositories that share one transaction, so their changes are all saved
    /// together or all discarded together. The repository list grows as new use cases
    /// need it; Commit, Rollback and disposal stay the same for every use case.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets the player repository bound to this unit of work.
        /// </summary>
        IPlayerRepository Players { get; }

        /// <summary>
        /// Gets the player account repository bound to this unit of work.
        /// </summary>
        IPlayerAccountRepository PlayerAccounts { get; }

        /// <summary>
        /// Gets the verification code repository bound to this unit of work.
        /// </summary>
        IVerificationCodeRepository VerificationCodes { get; }

        /// <summary>
        /// Gets the locale repository bound to this unit of work.
        /// </summary>
        ILocaleRepository Locales { get; }

        /// <summary>
        /// Gets the guest session repository bound to this unit of work.
        /// </summary>
        IGuestSessionRepository GuestSessions { get; }

        /// <summary>
        /// Gets the friend request and friendship repository bound to this unit of work.
        /// </summary>
        IFriendRepository Friends { get; }

        /// <summary>
        /// Gets the warning and ban repository bound to this unit of work.
        /// </summary>
        IModerationRepository Moderation { get; }

        /// <summary>
        /// Gets the chat message repository bound to this unit of work.
        /// </summary>
        IMessageRepository Messages { get; }

        /// <summary>
        /// Gets the challenge repository bound to this unit of work.
        /// </summary>
        IChallengeRepository Challenges { get; }

        /// <summary>
        /// Gets the match and match history repository bound to this unit of work.
        /// </summary>
        IGameRepository Games { get; }

        /// <summary>
        /// Saves every change made through this unit of work as one transaction, including
        /// changes made by calling methods on entities the repositories returned.
        /// </summary>
        void Commit();

        /// <summary>
        /// Discards every change made through this unit of work.
        /// </summary>
        void Rollback();
    }
}
