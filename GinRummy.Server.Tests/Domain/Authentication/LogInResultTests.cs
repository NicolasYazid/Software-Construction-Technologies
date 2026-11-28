using System;

using GinRummy.Domain.Entities.Authentication;
using NUnit.Framework;

namespace GinRummy.Server.Tests.Domain.Authentication
{
    [TestFixture]
    public class LogInResultTests
    {
        private const string SanctionReasonName = "Spam";
        private static readonly DateTime s_sanctionExpiresAt = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc);

        [Test]
        public void Test_ForRejected_InvalidCredentials_KeepsTheStatus()
        {
            LogInResult result = LogInResult.ForRejected(LogInStatus.InvalidCredentials);

            Assert.AreEqual(LogInStatus.InvalidCredentials, result.Status);
        }

        [Test]
        public void Test_ForRejected_InvalidCredentials_DoesNotRequireTwoFactor()
        {
            LogInResult result = LogInResult.ForRejected(LogInStatus.InvalidCredentials);

            Assert.IsFalse(result.IsTwoFactorRequired);
        }

        [Test]
        public void Test_ForBanned_ReasonAndEnd_ReturnsAccountBanned()
        {
            LogInResult result = LogInResult.ForBanned(SanctionReasonName, s_sanctionExpiresAt);

            Assert.AreEqual(LogInStatus.AccountBanned, result.Status);
        }

        [Test]
        public void Test_ForBanned_ReasonAndEnd_KeepsTheReason()
        {
            LogInResult result = LogInResult.ForBanned(SanctionReasonName, s_sanctionExpiresAt);

            Assert.AreEqual(SanctionReasonName, result.SanctionReasonName);
        }

        [Test]
        public void Test_ForBanned_ReasonAndEnd_KeepsTheEnd()
        {
            LogInResult result = LogInResult.ForBanned(SanctionReasonName, s_sanctionExpiresAt);

            Assert.AreEqual(s_sanctionExpiresAt, result.SanctionExpiresAt);
        }

        [Test]
        public void Test_ForPendingTwoFactor_NullAccount_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => LogInResult.ForPendingTwoFactor(null));
        }
    }
}
