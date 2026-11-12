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
        private readonly RoundRegistry _roundRegistry;
        private readonly List<PendingChallenge> _pendingChallenges;

        public ChallengeBoard(RoundRegistry roundRegistry)
        {
            _roundRegistry = roundRegistry ?? throw new ArgumentNullException(nameof(roundRegistry));
            _syncRoot = new object();
            _pendingChallenges = new List<PendingChallenge>();
        }

        public void Post(PendingChallenge challenge)
        {
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

                if (_pendingChallenges.Any(pending => pending.Challenger.Id == challenge.Challenger.Id))
                {
                    throw new PlayerUnavailableException("The challenger already has an open challenge.");
                }

                EnsureFree(challenge);
                _pendingChallenges.Add(challenge);
            }
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

        public ActiveRound Accept(int challengedId, int challengerPublicTag)
        {
            ActiveRound startedRound;
            lock (_syncRoot)
            {
                PendingChallenge foundChallenge = RemoveAddressedTo(challengedId, challengerPublicTag);
                if (foundChallenge == null)
                {
                    throw new ChallengeNotFoundException("The challenge no longer exists.");
                }

                EnsureFree(foundChallenge);
                startedRound = _roundRegistry.Start(foundChallenge.Challenger, foundChallenge.Challenged);
            }

            return startedRound;
        }

        private static bool IsReverseOf(PendingChallenge pending, PendingChallenge challenge)
        {
            return (pending.Challenger.Id == challenge.Challenged.Id) && (pending.Challenged.Id == challenge.Challenger.Id);
        }

        private void EnsureFree(PendingChallenge challenge)
        {
            bool isAnyPlaying = _roundRegistry.IsPlaying(challenge.Challenger.Id) || _roundRegistry.IsPlaying(challenge.Challenged.Id);
            if (isAnyPlaying)
            {
                throw new OpponentBusyException("One of the players is already in a round.");
            }
        }
    }
}