using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Domain;

namespace GinRummy.Application.UseCases.DirectDuel
{
    public class ChallengeBoard
    {
        private readonly object _syncRoot;
        private readonly MatchRegistry _matchRegistry;
        private readonly List<PendingChallenge> _pendingChallenges;

        public ChallengeBoard(MatchRegistry matchRegistry)
        {
            _matchRegistry = matchRegistry ?? throw new ArgumentNullException(nameof(matchRegistry));
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
                    _pendingChallenges.Add(challenge);
                }
            }

            return outcome;
        }

        public PendingChallenge RemoveFrom(int challengerId)
        {
            PendingChallenge removedChallenge;
            lock (_syncRoot)
            {
                removedChallenge = _pendingChallenges.FirstOrDefault(pending => pending.Challenger.Id == challengerId);
                _pendingChallenges.Remove(removedChallenge);
            }

            return removedChallenge;
        }

        public PendingChallenge RemoveAddressedTo(int challengedId, int challengerPublicTag)
        {
            PendingChallenge removedChallenge;
            lock (_syncRoot)
            {
                removedChallenge = _pendingChallenges.FirstOrDefault(
                    pending => (pending.Challenged.Id == challengedId) && (pending.Challenger.PublicTag == challengerPublicTag));
                _pendingChallenges.Remove(removedChallenge);
            }

            return removedChallenge;
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

                if (outcome == ChallengeOutcome.Done)
                {
                    startedMatch = _matchRegistry.Start(foundChallenge.Challenger, foundChallenge.Challenged);
                }
            }

            return outcome;
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