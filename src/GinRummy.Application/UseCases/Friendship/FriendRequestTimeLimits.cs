using System;
using System.Collections.Concurrent;

using GinRummy.Domain.Entities.Friendship;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.Friendship
{
    public class FriendRequestTimeLimits
    {
        private readonly IClock _requestTimesClock;
        private readonly ConcurrentDictionary<int, ConcurrentQueue<DateTime>> _sentRequestTimesBySender;
        private readonly ConcurrentDictionary<(int AffectedPlayerId, int DecidingPlayerId), DateTime> _rejectionOrRemovalTimes;

        public FriendRequestTimeLimits(IClock requestTimesClock)
        {
            _requestTimesClock = requestTimesClock ?? throw new ArgumentNullException(nameof(requestTimesClock));
            _sentRequestTimesBySender = new ConcurrentDictionary<int, ConcurrentQueue<DateTime>>();
            _rejectionOrRemovalTimes = new ConcurrentDictionary<(int AffectedPlayerId, int DecidingPlayerId), DateTime>();
        }

        public bool CanSendAnotherRequest(int senderId)
        {
            ConcurrentQueue<DateTime> sentRequestTimes = _sentRequestTimesBySender.GetOrAdd(senderId, new ConcurrentQueue<DateTime>());
            RemoveTimesOutsideCountingPeriod(sentRequestTimes);

            return sentRequestTimes.Count < FriendRequestRules.MaxRequestsPerPeriod;
        }

        public DateTime RegisterSentRequest(int senderId)
        {
            DateTime sentAt = _requestTimesClock.UtcNow;
            _sentRequestTimesBySender.GetOrAdd(senderId, new ConcurrentQueue<DateTime>()).Enqueue(sentAt);

            return sentAt;
        }

        public void RegisterRejectionOrRemoval(int affectedPlayerId, int decidingPlayerId)
        {
            _rejectionOrRemovalTimes[(affectedPlayerId, decidingPlayerId)] = _requestTimesClock.UtcNow;
        }

        public bool HasWaitingTimeEnded(int senderId, int addresseeId)
        {
            bool hasWaitingTimeEnded = true;
            DateTime rejectedOrRemovedAt;
            if (_rejectionOrRemovalTimes.TryGetValue((senderId, addresseeId), out rejectedOrRemovedAt))
            {
                DateTime waitingTimeEnd = rejectedOrRemovedAt.Add(FriendRequestRules.WaitingTimeAfterRejectionOrRemoval);
                hasWaitingTimeEnded = _requestTimesClock.UtcNow >= waitingTimeEnd;
            }

            return hasWaitingTimeEnded;
        }

        private void RemoveTimesOutsideCountingPeriod(ConcurrentQueue<DateTime> sentRequestTimes)
        {
            DateTime countingPeriodStart = _requestTimesClock.UtcNow.Subtract(FriendRequestRules.RequestCountingPeriod);
            DateTime oldestSentAt;
            while (sentRequestTimes.TryPeek(out oldestSentAt) && (oldestSentAt <= countingPeriodStart))
            {
                sentRequestTimes.TryDequeue(out oldestSentAt);
            }
        }
    }
}
