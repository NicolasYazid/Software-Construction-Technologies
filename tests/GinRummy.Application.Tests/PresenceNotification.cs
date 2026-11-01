using GinRummy.Domain.Enums;

namespace GinRummy.Application.Tests
{
    // Immutable record of one push, so the recording notifier can be queried by a test.
    internal class PresenceNotification
    {
        public PresenceNotification(int recipientPlayerId, int changedPlayerId, PresenceState state)
        {
            RecipientPlayerId = recipientPlayerId;
            ChangedPlayerId = changedPlayerId;
            State = state;
        }

        public int RecipientPlayerId { get; private set; }
        public int ChangedPlayerId { get; private set; }
        public PresenceState State { get; private set; }

        public bool Matches(int recipientPlayerId, int changedPlayerId, PresenceState state)
        {
            bool matches = (RecipientPlayerId == recipientPlayerId)
                && (ChangedPlayerId == changedPlayerId)
                && (State == state);

            return matches;
        }
    }
}
