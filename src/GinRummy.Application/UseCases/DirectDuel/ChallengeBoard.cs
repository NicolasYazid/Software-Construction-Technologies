using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.Ports.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Domain;

namespace GinRummy.Application.UseCases.DirectDuel
{
    public class ChallengeBoard
    {
        private readonly object _syncRoot;
        private readonly MatchRegistry _matchRegistry;
        private readonly IChallengeRecorder _challengeRecorder;
        private readonly List<PendingChallenge> _pendingChallenges;

        public ChallengeBoard(MatchRegistry matchRegistry, IChallengeRecorder challengeRecorder)
        {
            _matchRegistry = matchRegistry ?? throw new ArgumentNullException(nameof(matchRegistry));
            _challengeRecorder = challengeRecorder ?? throw new ArgumentNullException(nameof(challengeRecorder));
            _syncRoot = new object();
            _pendingChallenges = new List<PendingChallenge>();
        }

        public ChallengeOutcome Post(PendingChallenge challenge)
        {
            ChallengeOutcome outcome = ChallengeOutcome.AlreadyChallenging;
            lock (_syncRoot)
            {
                if (challenge.Challenger.Id == challenge.Challenged.Id)
                {
                    throw new SelfChallengeException("A player cannot challenge themselves.");
                }

                if (_pendingChallenges.Any(pending => IsReverseOf(pending, challenge)))
                {
                    throw new ConcurrentChallengeException("The challenged player already challenged the challenger.");
                }

                if (!_pendingChallenges.Any(pending => pending.Challenger.Id == challenge.Challenger.Id))
                {
                    outcome = CheckPlayersFree(challenge);
                }

                if (outcome == ChallengeOutcome.Done)
                {
                    _challengeRecorder.RecordSent(challenge);
                    _pendingChallenges.Add(challenge);
                }
            }

            return outcome;
        }

        public PendingChallenge Cancel(int challengerId)
        {
            PendingChallenge cancelledChallenge;
            lock (_syncRoot)
            {
                cancelledChallenge = _pendingChallenges.FirstOrDefault(pending => pending.Challenger.Id == challengerId);
                if (cancelledChallenge != null)
                {
                    _pendingChallenges.Remove(cancelledChallenge);
                    _challengeRecorder.RecordCancelled(cancelledChallenge);
                }
            }

            return cancelledChallenge;
        }

        public PendingChallenge Decline(int challengedId, int challengerPublicTag)
        {
            PendingChallenge declinedChallenge;
            lock (_syncRoot)
            {
                declinedChallenge = RemoveAddressedTo(challengedId, challengerPublicTag);
                if (declinedChallenge != null)
                {
                    _challengeRecorder.RecordDeclined(declinedChallenge);
                }
            }

            return declinedChallenge;
        }

        public ChallengeOutcome Accept(int challengedId, int challengerPublicTag, out ActiveMatch startedMatch)
        {
            ChallengeOutcome outcome = ChallengeOutcome.ChallengeExpired;
            startedMatch = null;
            lock (_syncRoot)
            {
                PendingChallenge foundChallenge = RemoveAddressedTo(challengedId, challengerPublicTag);
                if (foundChallenge != null)
                {
                    outcome = CheckPlayersFree(foundChallenge);
                }

                if (outcome == ChallengeOutcome.OpponentBusy)
                {
                    _challengeRecorder.RecordExpired(foundChallenge);
                }
                else if ((outcome == ChallengeOutcome.Done) && !_challengeRecorder.RecordAccepted(foundChallenge))
                {
                    outcome = ChallengeOutcome.ChallengeExpired;
                }

                if (outcome == ChallengeOutcome.Done)
                {
                    startedMatch = _matchRegistry.Start(foundChallenge.Challenger, foundChallenge.Challenged);
                }
            }

            return outcome;
        }

        private PendingChallenge RemoveAddressedTo(int challengedId, int challengerPublicTag)
        {
            PendingChallenge removedChallenge = _pendingChallenges.FirstOrDefault(
                pending => (pending.Challenged.Id == challengedId) && (pending.Challenger.PublicTag == challengerPublicTag));
            _pendingChallenges.Remove(removedChallenge);

            return removedChallenge;
        }

        private static bool IsReverseOf(PendingChallenge pending, PendingChallenge challenge)
        {
            return (pending.Challenger.Id == challenge.Challenged.Id) && (pending.Challenged.Id == challenge.Challenger.Id);
        }

        private ChallengeOutcome CheckPlayersFree(PendingChallenge challenge)
        {
            ChallengeOutcome outcome = ChallengeOutcome.Done;
            if (_matchRegistry.IsPlaying(challenge.Challenger.Id) || _matchRegistry.IsPlaying(challenge.Challenged.Id))
            {
                outcome = ChallengeOutcome.OpponentBusy;
            }

            return outcome;
        }
    }
}