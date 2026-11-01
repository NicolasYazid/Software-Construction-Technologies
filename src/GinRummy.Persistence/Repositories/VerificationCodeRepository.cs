using System;
using System.Linq;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Persistence.Persistence;

namespace GinRummy.Persistence.Repositories
{
    /// <summary>
    /// Fulfils IVerificationCodeRepository using Entity Framework against GinRummy_Dev.
    /// By default, opens a short-lived context per operation; optionally reuses a context
    /// someone else already opened, so several repositories can share one transaction.
    /// </summary>
    public class VerificationCodeRepository : IVerificationCodeRepository
    {
        // The counter is increased by the database itself so that two wrong attempts sent at the same time are both counted.
        private const string RegisterFailedAttemptCommand =
            "UPDATE dbo.VerificationCode "
            + "SET attempts = attempts + 1 "
            + "WHERE verification_code_id = @p0;";

        private const string MarkAsUsedCommand =
            "UPDATE dbo.VerificationCode "
            + "SET used_at = @p0 "
            + "WHERE verification_code_id = @p1;";

        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        /// <summary>
        /// Builds the repository so that each operation opens and closes its own context.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        /// <exception cref="ArgumentNullException">Thrown when the name is null.</exception>
        public VerificationCodeRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName
                ?? throw new ArgumentNullException(nameof(connectionStringName));
        }

        /// <summary>
        /// Builds the repository so that every operation runs on a context someone else
        /// already opened, so it can share a transaction with other repositories. The
        /// caller stays responsible for disposing that context.
        /// </summary>
        /// <param name="sharedContext">Context to reuse instead of opening a new one.</param>
        /// <exception cref="ArgumentNullException">Thrown when the context is null.</exception>
        public VerificationCodeRepository(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext ?? throw new ArgumentNullException(nameof(sharedContext));
        }

        /// <summary>
        /// Finds the most recently created code for a player and a purpose, or null
        /// when none exists.
        /// </summary>
        /// <param name="playerId">Player the code belongs to.</param>
        /// <param name="purpose">Reason the code was generated for.</param>
        /// <returns>The matching code, or null.</returns>
        public VerificationCode FindMostRecent(int playerId, VerificationPurpose purpose)
        {
            VerificationCode mostRecentCode;
            if (_sharedContext != null)
            {
                mostRecentCode = _sharedContext.VerificationCodes
                    .Where(verificationCode => verificationCode.PlayerId == playerId)
                    .Where(verificationCode => verificationCode.Purpose == purpose)
                    .OrderByDescending(verificationCode => verificationCode.CreatedAt)
                    .FirstOrDefault();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    mostRecentCode = context.VerificationCodes
                        .Where(verificationCode => verificationCode.PlayerId == playerId)
                        .Where(verificationCode => verificationCode.Purpose == purpose)
                        .OrderByDescending(verificationCode => verificationCode.CreatedAt)
                        .FirstOrDefault();
                }
            }

            return mostRecentCode;
        }

        /// <summary>
        /// Adds a new code and persists it immediately.
        /// </summary>
        /// <param name="newVerificationCode">Code to create.</param>
        public void Add(VerificationCode newVerificationCode)
        {
            if (_sharedContext != null)
            {
                _sharedContext.VerificationCodes.Add(newVerificationCode);
                _sharedContext.SaveChanges();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    context.VerificationCodes.Add(newVerificationCode);
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Records the email sent to deliver a verification code, whether the delivery worked
        /// or failed, and persists it immediately.
        /// </summary>
        /// <param name="newEmail">Email to record.</param>
        public void AddEmail(Email newEmail)
        {
            if (_sharedContext != null)
            {
                _sharedContext.Emails.Add(newEmail);
                _sharedContext.SaveChanges();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    context.Emails.Add(newEmail);
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Adds one to the failed-attempt counter of the given code.
        /// </summary>
        /// <param name="verificationCodeId">Code that received a wrong guess.</param>
        public void RegisterFailedAttempt(int verificationCodeId)
        {
            if (_sharedContext != null)
            {
                _sharedContext.Database.ExecuteSqlCommand(
                    RegisterFailedAttemptCommand,
                    verificationCodeId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    context.Database.ExecuteSqlCommand(
                        RegisterFailedAttemptCommand,
                        verificationCodeId);
                }
            }
        }

        /// <summary>
        /// Marks the given code as used at the given moment.
        /// </summary>
        /// <param name="verificationCodeId">Code that was successfully verified.</param>
        /// <param name="usedAt">Moment the code was used, in UTC, read from the server clock.</param>
        public void MarkAsUsed(int verificationCodeId, DateTime usedAt)
        {
            if (_sharedContext != null)
            {
                _sharedContext.Database.ExecuteSqlCommand(
                    MarkAsUsedCommand,
                    usedAt,
                    verificationCodeId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    context.Database.ExecuteSqlCommand(
                        MarkAsUsedCommand,
                        usedAt,
                        verificationCodeId);
                }
            }
        }
    }
}
