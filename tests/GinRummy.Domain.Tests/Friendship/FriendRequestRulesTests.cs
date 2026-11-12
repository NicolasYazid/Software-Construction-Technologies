using System;

using GinRummy.Domain.Entities.Friendship;
using NUnit.Framework;

namespace GinRummy.Domain.Tests.Friendship
{
    [TestFixture]
    public class FriendRequestRulesTests
    {
        private const int SenderId = 1;
        private const int AddresseeId = 2;
        private const int FirstOtherPlayerId = 100;

        private static readonly DateTime _requestCreatedAt = new DateTime(2026, 11, 12, 10, 0, 0, DateTimeKind.Utc);

        private FakeFriendRepository _friendRepository;

        [SetUp]
        public void SetUp()
        {
            _friendRepository = new FakeFriendRepository();
        }

        [Test]
        public void Test_CheckNewRequest_NoRuleBroken_ReturnsSent()
        {
            FriendRequestResult result = FriendRequestRules.CheckNewRequest(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResult.Sent, result);
        }

        [Test]
        public void Test_CheckNewRequest_RequestToYourself_ReturnsSelfRequest()
        {
            FriendRequestResult result = FriendRequestRules.CheckNewRequest(_friendRepository, SenderId, SenderId);

            Assert.AreEqual(FriendRequestResult.SelfRequest, result);
        }

        [Test]
        public void Test_CheckNewRequest_PlayersAreFriends_ReturnsAlreadyFriends()
        {
            _friendRepository.AddFriendShip(new FriendShip(SenderId, AddresseeId));

            FriendRequestResult result = FriendRequestRules.CheckNewRequest(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResult.AlreadyFriends, result);
        }

        [Test]
        public void Test_CheckNewRequest_SenderAlreadySentRequest_ReturnsRequestAlreadySent()
        {
            _friendRepository.AddRequest(new FriendRequest(SenderId, AddresseeId, _requestCreatedAt));

            FriendRequestResult result = FriendRequestRules.CheckNewRequest(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResult.RequestAlreadySent, result);
        }

        [Test]
        public void Test_CheckNewRequest_AddresseeAlreadySentRequest_ReturnsIncomingRequestExists()
        {
            _friendRepository.AddRequest(new FriendRequest(AddresseeId, SenderId, _requestCreatedAt));

            FriendRequestResult result = FriendRequestRules.CheckNewRequest(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResult.IncomingRequestExists, result);
        }

        [Test]
        public void Test_CheckNewRequest_SenderHasMaxFriends_ReturnsSenderFriendLimitReached()
        {
            AddFriendsTo(SenderId, FriendRequestRules.MaxFriendsPerPlayer);

            FriendRequestResult result = FriendRequestRules.CheckNewRequest(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResult.SenderFriendLimitReached, result);
        }

        [Test]
        public void Test_CheckNewRequest_SenderHasOneFriendLessThanMax_ReturnsSent()
        {
            AddFriendsTo(SenderId, FriendRequestRules.MaxFriendsPerPlayer - 1);

            FriendRequestResult result = FriendRequestRules.CheckNewRequest(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResult.Sent, result);
        }

        [Test]
        public void Test_CheckNewRequest_AddresseeHasMaxFriends_ReturnsAddresseeFriendLimitReached()
        {
            AddFriendsTo(AddresseeId, FriendRequestRules.MaxFriendsPerPlayer);

            FriendRequestResult result = FriendRequestRules.CheckNewRequest(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResult.AddresseeFriendLimitReached, result);
        }

        [Test]
        public void Test_CheckNewRequest_AddresseeHasMaxPendingRequests_ReturnsAddresseeHasTooManyRequests()
        {
            AddRequestsReceivedBy(AddresseeId, FriendRequestRules.MaxPendingRequestsPerPlayer);

            FriendRequestResult result = FriendRequestRules.CheckNewRequest(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResult.AddresseeHasTooManyRequests, result);
        }

        [Test]
        public void Test_CheckNewRequest_AddresseeHasOneRequestLessThanMax_ReturnsSent()
        {
            AddRequestsReceivedBy(AddresseeId, FriendRequestRules.MaxPendingRequestsPerPlayer - 1);

            FriendRequestResult result = FriendRequestRules.CheckNewRequest(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResult.Sent, result);
        }

        [Test]
        public void Test_CheckNewRequest_NullRepository_ThrowsArgumentNullException()
        {
            TestDelegate checkWithoutRepository = () => FriendRequestRules.CheckNewRequest(null, SenderId, AddresseeId);

            Assert.Throws<ArgumentNullException>(checkWithoutRepository);
        }

        [Test]
        public void Test_CheckAcceptance_NoLimitReached_ReturnsAccepted()
        {
            FriendRequestResponseResult result = FriendRequestRules.CheckAcceptance(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResponseResult.Accepted, result);
        }

        [Test]
        public void Test_CheckAcceptance_RequesterHasMaxFriends_ReturnsRequesterFriendLimitReached()
        {
            AddFriendsTo(SenderId, FriendRequestRules.MaxFriendsPerPlayer);

            FriendRequestResponseResult result = FriendRequestRules.CheckAcceptance(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResponseResult.RequesterFriendLimitReached, result);
        }

        [Test]
        public void Test_CheckAcceptance_RequesterHasOneFriendLessThanMax_ReturnsAccepted()
        {
            AddFriendsTo(SenderId, FriendRequestRules.MaxFriendsPerPlayer - 1);

            FriendRequestResponseResult result = FriendRequestRules.CheckAcceptance(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResponseResult.Accepted, result);
        }

        [Test]
        public void Test_CheckAcceptance_AddresseeHasMaxFriends_ReturnsAddresseeFriendLimitReached()
        {
            AddFriendsTo(AddresseeId, FriendRequestRules.MaxFriendsPerPlayer);

            FriendRequestResponseResult result = FriendRequestRules.CheckAcceptance(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResponseResult.AddresseeFriendLimitReached, result);
        }

        [Test]
        public void Test_CheckAcceptance_AddresseeHasOneFriendLessThanMax_ReturnsAccepted()
        {
            AddFriendsTo(AddresseeId, FriendRequestRules.MaxFriendsPerPlayer - 1);

            FriendRequestResponseResult result = FriendRequestRules.CheckAcceptance(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResponseResult.Accepted, result);
        }

        [Test]
        public void Test_CheckAcceptance_BothHaveMaxFriends_ReturnsRequesterFriendLimitReached()
        {
            AddFriendsTo(SenderId, FriendRequestRules.MaxFriendsPerPlayer);
            AddFriendsTo(AddresseeId, FriendRequestRules.MaxFriendsPerPlayer);

            FriendRequestResponseResult result = FriendRequestRules.CheckAcceptance(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResponseResult.RequesterFriendLimitReached, result);
        }

        [Test]
        public void Test_CheckAcceptance_PlayersAreFriendsAndBothHaveMaxFriends_ReturnsAccepted()
        {
            _friendRepository.AddFriendShip(new FriendShip(SenderId, AddresseeId));
            AddFriendsTo(SenderId, FriendRequestRules.MaxFriendsPerPlayer - 1);
            AddFriendsTo(AddresseeId, FriendRequestRules.MaxFriendsPerPlayer - 1);

            FriendRequestResponseResult result = FriendRequestRules.CheckAcceptance(_friendRepository, SenderId, AddresseeId);

            Assert.AreEqual(FriendRequestResponseResult.Accepted, result);
        }

        [Test]
        public void Test_CheckAcceptance_NullRepository_ThrowsArgumentNullException()
        {
            TestDelegate checkWithoutRepository = () => FriendRequestRules.CheckAcceptance(null, SenderId, AddresseeId);

            Assert.Throws<ArgumentNullException>(checkWithoutRepository);
        }

        private void AddFriendsTo(int playerAccountId, int friendCount)
        {
            for (int i = 0; i < friendCount; i++)
            {
                _friendRepository.AddFriendShip(new FriendShip(playerAccountId, FirstOtherPlayerId + i));
            }
        }

        private void AddRequestsReceivedBy(int addresseePlayerAccountId, int requestCount)
        {
            for (int i = 0; i < requestCount; i++)
            {
                _friendRepository.AddRequest(new FriendRequest(FirstOtherPlayerId + i, addresseePlayerAccountId, _requestCreatedAt));
            }
        }
    }
}
