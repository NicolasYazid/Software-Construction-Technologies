using System;
using System.Collections.Generic;

using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Domain;

namespace GinRummy.Application.UseCases.Gameplay
{
    public class RoundRegistry
    {
        private readonly object _syncRoot;
        private readonly Random _random;
        private readonly Dictionary<int, ActiveRound> _roundsByPlayerId;

        public RoundRegistry()
        {
            _syncRoot = new object();
            _random = new Random();
            _roundsByPlayerId = new Dictionary<int, ActiveRound>();
        }

        public ActiveRound Start(DuelPlayer dealer, DuelPlayer nonDealer)
        {
            GameRound round = new GameRound(dealer.Id, nonDealer.Id, StandardDeck.CreateShuffled(_random));
            ActiveRound activeRound = new ActiveRound(round, dealer, nonDealer);
            lock (_syncRoot)
            {
                _roundsByPlayerId[dealer.Id] = activeRound;
                _roundsByPlayerId[nonDealer.Id] = activeRound;
            }

            return activeRound;
        }

        public ActiveRound Find(int playerId)
        {
            ActiveRound foundRound;
            lock (_syncRoot)
            {
                _roundsByPlayerId.TryGetValue(playerId, out foundRound);
            }

            return foundRound;
        }

        public bool IsPlaying(int playerId)
        {
            return Find(playerId) != null;
        }

        public bool Remove(ActiveRound activeRound)
        {
            bool isRemoved;
            lock (_syncRoot)
            {
                ActiveRound registeredRound;
                _roundsByPlayerId.TryGetValue(activeRound.Players[0].Id, out registeredRound);
                isRemoved = ReferenceEquals(registeredRound, activeRound);
                if (isRemoved)
                {
                    RemovePlayersOf(activeRound);
                }
            }

            return isRemoved;
        }

        private void RemovePlayersOf(ActiveRound activeRound)
        {
            foreach (DuelPlayer player in activeRound.Players)
            {
                _roundsByPlayerId.Remove(player.Id);
            }
        }
    }
}
