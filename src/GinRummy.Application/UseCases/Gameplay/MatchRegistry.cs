using System;
using System.Collections.Generic;

using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Domain;

namespace GinRummy.Application.UseCases.Gameplay
{
    public class MatchRegistry
    {
        private readonly object _syncRoot;
        private readonly Func<IList<Card>> _createShuffledDeck;
        private readonly Dictionary<int, ActiveMatch> _matchesByPlayerId;

        public MatchRegistry(Func<IList<Card>> createShuffledDeck)
        {
            _createShuffledDeck = createShuffledDeck ?? throw new ArgumentNullException(nameof(createShuffledDeck));
            _syncRoot = new object();
            _matchesByPlayerId = new Dictionary<int, ActiveMatch>();
        }

        public ActiveMatch Start(DuelPlayer dealer, DuelPlayer nonDealer)
        {
            GameRound firstRound = new GameRound(dealer.Id, nonDealer.Id, ShuffleDeck());
            ActiveMatch activeMatch = new ActiveMatch(new GameMatch(firstRound), dealer, nonDealer);
            lock (_syncRoot)
            {
                _matchesByPlayerId[dealer.Id] = activeMatch;
                _matchesByPlayerId[nonDealer.Id] = activeMatch;
            }

            return activeMatch;
        }

        public void DealNextRound(ActiveMatch activeMatch)
        {
            activeMatch.DealNextRound(ShuffleDeck());
        }

        public ActiveMatch Find(int playerId)
        {
            ActiveMatch foundMatch;
            lock (_syncRoot)
            {
                _matchesByPlayerId.TryGetValue(playerId, out foundMatch);
            }

            return foundMatch;
        }

        public bool IsPlaying(int playerId)
        {
            return Find(playerId) != null;
        }

        public bool Remove(ActiveMatch activeMatch)
        {
            bool isRemoved;
            lock (_syncRoot)
            {
                ActiveMatch registeredMatch;
                _matchesByPlayerId.TryGetValue(activeMatch.Players[0].Id, out registeredMatch);
                isRemoved = ReferenceEquals(registeredMatch, activeMatch);
                if (isRemoved)
                {
                    RemovePlayersOf(activeMatch);
                }
            }

            return isRemoved;
        }

        private IList<Card> ShuffleDeck()
        {
            IList<Card> shuffledDeck;
            lock (_syncRoot)
            {
                shuffledDeck = _createShuffledDeck();
            }

            return shuffledDeck;
        }

        private void RemovePlayersOf(ActiveMatch activeMatch)
        {
            foreach (DuelPlayer player in activeMatch.Players)
            {
                _matchesByPlayerId.Remove(player.Id);
            }
        }
    }
}
