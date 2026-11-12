using System;

using GinRummy.Application.UseCases.Friendship;
using GinRummy.Domain.Entities.Friendship;
using NUnit.Framework;

namespace GinRummy.Application.Tests.Friendship
{
    [TestFixture]
    public class FriendRequestTimeLimitsTests
    {
        private const int SenderId = 1;
        private const int AddresseeId = 2;
        private const int ThirdPlayerId = 3;

        private static readonly DateTime _startMoment = new DateTime(2026, 11, 12, 10, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan _oneSecond = TimeSpan.FromSeconds(1);

        private FakeClock _fakeClock;
        private FriendRequestTimeLimits _timeLimits;

        [SetUp]
        public void SetUp()
        {
            _fakeClock = new FakeClock(_startMoment);
            _timeLimits = new FriendRequestTimeLimits(_fakeClock);
        }

        [Test]
        public void Test_CanSendAnotherRequest_NoRequestSent_ReturnsTrue()
        {
            bool canSend = _timeLimits.CanSendAnotherRequest(SenderId);

            Assert.IsTrue(canSend);
        }

        [Test]
        public void Test_CanSendAnotherRequest_OneRequestLessThanLimit_ReturnsTrue()
        {
            RegisterSentRequests(SenderId, FriendRequestRules.MaxRequestsPerPeriod - 1);

            bool canSend = _timeLimits.CanSendAnotherRequest(SenderId);

            Assert.IsTrue(canSend);
        }

        [Test]
        public void Test_CanSendAnotherRequest_LimitReachedWithinOneMinute_ReturnsFalse()
        {
            RegisterSentRequests(SenderId, FriendRequestRules.MaxRequestsPerPeriod);

            bool canSend = _timeLimits.CanSendAnotherRequest(SenderId);

            Assert.IsFalse(canSend);
        }

        [Test]
        public void Test_CanSendAnotherRequest_LimitReachedOneSecondBeforeMinutePasses_ReturnsFalse()
        {
            RegisterSentRequests(SenderId, FriendRequestRules.MaxRequestsPerPeriod);
            _fakeClock.MoveForward(FriendRequestRules.RequestCountingPeriod.Subtract(_oneSecond));

            bool canSend = _timeLimits.CanSendAnotherRequest(SenderId);

            Assert.IsFalse(canSend);
        }

        [Test]
        public void Test_CanSendAnotherRequest_LimitReachedAndOneMinutePassed_ReturnsTrue()
        {
            RegisterSentRequests(SenderId, FriendRequestRules.MaxRequestsPerPeriod);
            _fakeClock.MoveForward(FriendRequestRules.RequestCountingPeriod);

            bool canSend = _timeLimits.CanSendAnotherRequest(SenderId);

            Assert.IsTrue(canSend);
        }

        [Test]
        public void Test_CanSendAnotherRequest_LimitReachedByAnotherPlayer_ReturnsTrue()
        {
            RegisterSentRequests(ThirdPlayerId, FriendRequestRules.MaxRequestsPerPeriod);

            bool canSend = _timeLimits.CanSendAnotherRequest(SenderId);

            Assert.IsTrue(canSend);
        }

        [Test]
        public void Test_RegisterSentRequest_AnyMoment_ReturnsCurrentMoment()
        {
            DateTime sentAt = _timeLimits.RegisterSentRequest(SenderId);

            Assert.AreEqual(_startMoment, sentAt);
        }

        [Test]
        public void Test_HasWaitingTimeEnded_NoRejectionOrRemoval_ReturnsTrue()
        {
            bool hasWaitingTimeEnded = _timeLimits.HasWaitingTimeEnded(SenderId, AddresseeId);

            Assert.IsTrue(hasWaitingTimeEnded);
        }

        [Test]
        public void Test_HasWaitingTimeEnded_RejectedJustNow_ReturnsFalse()
        {
            _timeLimits.RegisterRejectionOrRemoval(SenderId, AddresseeId);

            bool hasWaitingTimeEnded = _timeLimits.HasWaitingTimeEnded(SenderId, AddresseeId);

            Assert.IsFalse(hasWaitingTimeEnded);
        }

        [Test]
        public void Test_HasWaitingTimeEnded_OneSecondBeforeWaitingTimePasses_ReturnsFalse()
        {
            _timeLimits.RegisterRejectionOrRemoval(SenderId, AddresseeId);
            _fakeClock.MoveForward(FriendRequestRules.WaitingTimeAfterRejectionOrRemoval.Subtract(_oneSecond));

            bool hasWaitingTimeEnded = _timeLimits.HasWaitingTimeEnded(SenderId, AddresseeId);

            Assert.IsFalse(hasWaitingTimeEnded);
        }

        [Test]
        public void Test_HasWaitingTimeEnded_WaitingTimePassed_ReturnsTrue()
        {
            _timeLimits.RegisterRejectionOrRemoval(SenderId, AddresseeId);
            _fakeClock.MoveForward(FriendRequestRules.WaitingTimeAfterRejectionOrRemoval);

            bool hasWaitingTimeEnded = _timeLimits.HasWaitingTimeEnded(SenderId, AddresseeId);

            Assert.IsTrue(hasWaitingTimeEnded);
        }

        [Test]
        public void Test_HasWaitingTimeEnded_PlayerWhoRejected_ReturnsTrue()
        {
            _timeLimits.RegisterRejectionOrRemoval(SenderId, AddresseeId);

            bool hasWaitingTimeEnded = _timeLimits.HasWaitingTimeEnded(AddresseeId, SenderId);

            Assert.IsTrue(hasWaitingTimeEnded);
        }

        private void RegisterSentRequests(int senderId, int requestCount)
        {
            for (int i = 0; i < requestCount; i++)
            {
                _timeLimits.RegisterSentRequest(senderId);
            }
        }
    }
}
