using System;
using System.Linq;

using GinRummy.Data.EntityFramework.Persistence;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;

namespace GinRummy.Data.EntityFramework.Repositories
{
    public class VerificationCodeRepository : IVerificationCodeRepository
    {
        private readonly string _connectionStringName;
        private readonly GinRummyContext _sharedContext;

        public VerificationCodeRepository(string connectionStringName)
        {
            _connectionStringName = connectionStringName;
        }

        // The context can come from the caller so that several repositories take part in one transaction.
        // Only the contexts this repository creates are disposed of here; a shared one belongs to its caller.
        public VerificationCodeRepository(GinRummyContext sharedContext)
        {
            _sharedContext = sharedContext;
        }

        public VerificationCode FindMostRecent(int playerId, VerificationPurpose purpose)
        {
            VerificationCode mostRecentCode;
            if (_sharedContext != null)
            {
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
