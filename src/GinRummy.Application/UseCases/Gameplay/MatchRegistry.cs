using System;
using System.Collections.Generic;

using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Domain;

namespace GinRummy.Application.UseCases.Gameplay
{
    public class MatchRegistry
    {
        private readonly object _syncRoot;
        private readonly Random _random;
        private readonly Dictionary<int, ActiveMatch> _matchesByPlayerId;

        public MatchRegistry()
        {
            _syncRoot = new object();
            _random = new Random();
            _matchesByPlayerId = new Dictionary<int, ActiveMatch>();
        }

        public ActiveMatch Start(DuelPlayer dealer, DuelPlayer nonDealer)
        {
            GameRound round = new GameRound(dealer.Id, nonDealer.Id, StandardDeck.CreateShuffled(_random));
            ActiveMatch activeMatch = new ActiveMatch(round, dealer, nonDealer);
            lock (_syncRoot)
            {
                _matchesByPlayerId[dealer.Id] = activeMatch;
                _matchesByPlayerId[nonDealer.Id] = activeMatch;
            }

            return activeMatch;
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

        private void RemovePlayersOf(ActiveMatch activeMatch)
        {
            foreach (DuelPlayer player in activeMatch.Players)
            {
                _matchesByPlayerId.Remove(player.Id);
            }
        }
    }
}
