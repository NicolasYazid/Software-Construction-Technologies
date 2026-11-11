using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.UseCases.Gameplay;

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

        public bool TryPost(PendingChallenge challenge)
        {
            bool isPosted;
            lock (_syncRoot)
            {
                bool areDifferentPlayers = challenge.Challenger.Id != challenge.Challenged.Id;
                bool hasOpenChallenge = _pendingChallenges.Any(pending => pending.Challenger.Id == challenge.Challenger.Id);
                isPosted = areDifferentPlayers && !hasOpenChallenge && AreFree(challenge);
                if (isPosted)
                {
                    _pendingChallenges.Add(challenge);
                }
            }

            return isPosted;
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
            ActiveRound startedRound = null;
            lock (_syncRoot)
            {
                PendingChallenge foundChallenge = RemoveAddressedTo(challengedId, challengerPublicTag);
                bool canStartRound = (foundChallenge != null) && AreFree(foundChallenge);
                if (canStartRound)
                {
                    startedRound = _roundRegistry.Start(foundChallenge.Challenger, foundChallenge.Challenged);
                }
            }

            return startedRound;
        }

        private bool AreFree(PendingChallenge challenge)
        {
            bool isChallengerFree = !_roundRegistry.IsPlaying(challenge.Challenger.Id);
            bool isChallengedFree = !_roundRegistry.IsPlaying(challenge.Challenged.Id);

            return isChallengerFree && isChallengedFree;
        }
    }
}
