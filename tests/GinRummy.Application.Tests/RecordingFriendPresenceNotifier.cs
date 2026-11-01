using System.Collections.Generic;

using GinRummy.Application.Ports;
using GinRummy.Domain.Enums;

namespace GinRummy.Application.Tests
{
    // Test double that records every push so a test can assert what the coordinator sent.
    internal class RecordingFriendPresenceNotifier : IFriendPresenceNotificationPort
    {
        private readonly List<PresenceNotification> _notifications;

        public RecordingFriendPresenceNotifier()
        {
            _notifications = new List<PresenceNotification>();
        }

        public int NotificationCount
        {
            get { return _notifications.Count; }
        }

        public void NotifyPresenceChanged(int recipientPlayerId, int changedPlayerId, PresenceState state)
        {
            _notifications.Add(new PresenceNotification(recipientPlayerId, changedPlayerId, state));
        }

        public bool HasNotification(int recipientPlayerId, int changedPlayerId, PresenceState state)
        {
            bool hasNotification = false;
            foreach (PresenceNotification notification in _notifications)
            {
                if (notification.Matches(recipientPlayerId, changedPlayerId, state))
                {
                    hasNotification = true;
                }
            }

            return hasNotification;
        }
    }
}
