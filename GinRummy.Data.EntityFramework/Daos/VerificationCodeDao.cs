using System;
using System.Linq;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Daos
{
    // Fulfils IVerificationCodeDao using Entity Framework against GinRummy_Dev.
    // By default, it opens a short-lived context per operation.
    // It can also reuse a context opened elsewhere, so several DAOs can share one transaction.
    public class VerificationCodeDao : IVerificationCodeDao
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        public VerificationCodeDao(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        public VerificationCodeDao(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext;
        }

        public VerificationCode FindMostRecent(int playerId, VerificationPurpose purpose)
        {
            VerificationCode mostRecentCode;
            if (_sharedContext != null)
            {
                // The caller owns this context.
                // It is disposed of when the caller's transaction ends.
                mostRecentCode = _sharedContext.VerificationCodes
                    .Where(code => (code.PlayerId == playerId) && (code.Purpose == purpose))
                    .OrderByDescending(code => code.CreatedAt)
                    .FirstOrDefault();
            }
            else
            {
                using (GinRummyContext context = new GinRummyContext(_connectionStringName))
                {
                    mostRecentCode = context.VerificationCodes
                        .Where(code => (code.PlayerId == playerId) && (code.Purpose == purpose))
                        .OrderByDescending(code => code.CreatedAt)
                        .FirstOrDefault();
                }
            }

            return mostRecentCode;
        }

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
