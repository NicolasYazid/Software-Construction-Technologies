using System.Collections.Generic;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Enums;
using NUnit.Framework;

namespace GinRummy.Application.Tests
{
    public class PresenceCoordinatorTests
    {
        private const int SubscriberId = 1;
        private const int FriendId = 2;

        private FakeFriendRepository _friendRepository;
        private RecordingFriendPresenceNotifier _notifier;
        private PresenceCoordinator _coordinator;

        [SetUp]
        public void SetUp()
        {
            _friendRepository = new FakeFriendRepository();
            _notifier = new RecordingFriendPresenceNotifier();
            _coordinator = new PresenceCoordinator(_friendRepository, _notifier);
        }

        [Test]
        public void Test_SubscribeToFriendsPresence_FriendIsConnected_FriendIsOnlineInSnapshot()
        {
            _friendRepository.AddFriendShip(new FriendShip(SubscriberId, FriendId));
            _coordinator.SubscribeToFriendsPresence(FriendId);

            IReadOnlyDictionary<int, PresenceState> snapshot = _coordinator.SubscribeToFriendsPresence(SubscriberId);

            Assert.AreEqual(PresenceState.Online, snapshot[FriendId]);
        }

        [Test]
        public void Test_SubscribeToFriendsPresence_FriendIsNotConnected_FriendIsOfflineInSnapshot()
        {
            _friendRepository.AddFriendShip(new FriendShip(SubscriberId, FriendId));

            IReadOnlyDictionary<int, PresenceState> snapshot = _coordinator.SubscribeToFriendsPresence(SubscriberId);

            Assert.AreEqual(PresenceState.Offline, snapshot[FriendId]);
        }

        [Test]
        public void Test_SubscribeToFriendsPresence_FriendIsConnected_PushesOnlineToFriend()
        {
            _friendRepository.AddFriendShip(new FriendShip(SubscriberId, FriendId));
            _coordinator.SubscribeToFriendsPresence(FriendId);

            _coordinator.SubscribeToFriendsPresence(SubscriberId);

            Assert.IsTrue(_notifier.HasNotification(FriendId, SubscriberId, PresenceState.Online));
        }

        [Test]
        public void Test_SubscribeToFriendsPresence_NoFriendConnected_PushesNothing()
        {
            _friendRepository.AddFriendShip(new FriendShip(SubscriberId, FriendId));

            _coordinator.SubscribeToFriendsPresence(SubscriberId);

            Assert.AreEqual(0, _notifier.NotificationCount);
        }

        [Test]
        public void Test_UnsubscribeFromFriendsPresence_FriendIsConnected_PushesOfflineToFriend()
        {
            _friendRepository.AddFriendShip(new FriendShip(SubscriberId, FriendId));
            _coordinator.SubscribeToFriendsPresence(FriendId);
            _coordinator.SubscribeToFriendsPresence(SubscriberId);

            _coordinator.UnsubscribeFromFriendsPresence(SubscriberId);

            Assert.IsTrue(_notifier.HasNotification(FriendId, SubscriberId, PresenceState.Offline));
        }

        [Test]
        public void Test_SubscribeToFriendsPresence_PlayerHasNoFriends_ReturnsEmptySnapshot()
        {
            IReadOnlyDictionary<int, PresenceState> snapshot = _coordinator.SubscribeToFriendsPresence(SubscriberId);

            Assert.AreEqual(0, snapshot.Count);
        }
    }
}
