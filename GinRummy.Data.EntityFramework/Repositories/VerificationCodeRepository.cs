using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;

namespace GinRummy.Data.EntityFramework.Repositories
{
    /// <summary>
    /// Fulfils IVerificationCodeRepository using Entity Framework against GinRummy_Dev.
    /// By default, opens a short-lived context per operation; optionally reuses a context
    /// someone else already opened, so several repositories can share one transaction.
    /// </summary>
    public class VerificationCodeRepository : IVerificationCodeRepository
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        /// <summary>
        /// Builds the repository so that each operation opens and closes its own context.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        public VerificationCodeRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        /// <summary>
        /// Builds the repository so that every operation runs on a context someone else
        /// already opened, so it can share a transaction with other repositories. The
        /// caller stays responsible for disposing that context.
        /// </summary>
        /// <param name="sharedContext">Context to reuse instead of opening a new one.</param>
        public VerificationCodeRepository(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext;
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
                    .Where(verificationCode => verificationCode.PlayerId == playerId
                        && verificationCode.Purpose == purpose)
                    .OrderByDescending(verificationCode => verificationCode.CreatedAt)
                    .FirstOrDefault();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    mostRecentCode = context.VerificationCodes
                        .Where(verificationCode => verificationCode.PlayerId == playerId
                            && verificationCode.Purpose == purpose)
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
        /// Adds one to the failed-attempt counter of the given code.
        /// </summary>
        /// <param name="verificationCodeId">Code that received a wrong guess.</param>
        public void RegisterFailedAttempt(int verificationCodeId)
        {
            if (_sharedContext != null)
            {
                _sharedContext.Database.ExecuteSqlCommand(
                    "UPDATE VerificationCode SET attempts = attempts + 1 WHERE verification_code_id = {0}",
                    verificationCodeId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    context.Database.ExecuteSqlCommand(
                        "UPDATE VerificationCode SET attempts = attempts + 1 WHERE verification_code_id = {0}",
                        verificationCodeId);
                }
            }
        }

        /// <summary>
        /// Marks the given code as used, right now.
        /// </summary>
        /// <param name="verificationCodeId">Code that was successfully verified.</param>
        public void MarkAsUsed(int verificationCodeId)
        {
            if (_sharedContext != null)
            {
                _sharedContext.Database.ExecuteSqlCommand(
                    "UPDATE VerificationCode SET used_at = {0} WHERE verification_code_id = {1}",
                    DateTime.UtcNow,
                    verificationCodeId);
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    context.Database.ExecuteSqlCommand(
                        "UPDATE VerificationCode SET used_at = {0} WHERE verification_code_id = {1}",
                        DateTime.UtcNow,
                        verificationCodeId);
                }
            }
        }
    }
}
