using System;

using GinRummy.Application.Ports.Gameplay;
using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Domain.Gameplay;

namespace GinRummy.Application.UseCases.Gameplay
{
    public class PlayRoundUseCase : IPlayRoundUseCase
    {
        private readonly RoundRegistry _roundRegistry;
        private readonly IRoundAnnouncer _roundAnnouncer;

        public PlayRoundUseCase(RoundRegistry roundRegistry, IRoundAnnouncer roundAnnouncer)
        {
            _roundRegistry = roundRegistry ?? throw new ArgumentNullException(nameof(roundRegistry));
            _roundAnnouncer = roundAnnouncer ?? throw new ArgumentNullException(nameof(roundAnnouncer));
        }

        public void SendCurrentRound(int playerId)
        {
            ActiveRound activeRound = _roundRegistry.Find(playerId);
            if (activeRound != null)
            {
                lock (activeRound.SyncRoot)
                {
                    _roundAnnouncer.AnnounceRoundChanged(playerId, activeRound);
                }
            }
        }

        public void TakeFirstCard(int playerId)
        {
            ApplyMove(playerId, activeRound => activeRound.TakeFirstCard(playerId));
        }

        public void PassFirstCard(int playerId)
        {
            ApplyMove(playerId, activeRound => activeRound.PassFirstCard(playerId));
        }

        public void DrawFromStock(int playerId)
        {
            ApplyMove(playerId, activeRound => activeRound.DrawFromStock(playerId));
        }

        public void DrawFromDiscardPile(int playerId)
        {
            ApplyMove(playerId, activeRound => activeRound.DrawFromDiscardPile(playerId));
        }

        public void Discard(int playerId, Card card)
        {
            ApplyMove(playerId, activeRound => activeRound.Discard(playerId, card));
        }

        public void Knock(int playerId, Card discardedCard)
        {
            ApplyMove(playerId, activeRound => activeRound.Knock(playerId, discardedCard));
        }

        public void ForfeitRound(int playerId)
        {
            ActiveRound activeRound = _roundRegistry.Find(playerId);
            if ((activeRound != null) && _roundRegistry.Remove(activeRound))
            {
                _roundAnnouncer.AnnounceOpponentLeft(activeRound.FindOpponent(playerId).Id);
            }
        }

        private void ApplyMove(int playerId, Action<ActiveRound> play)
        {
            ActiveRound activeRound = _roundRegistry.Find(playerId);
            if (activeRound == null)
            {
                throw new InvalidMoveException("The player is not playing a round.");
            }

            lock (activeRound.SyncRoot)
            {
                play(activeRound);
                bool isFinished = activeRound.Round.Phase == TurnPhase.Finished;
                if (isFinished)
                {
                    _roundRegistry.Remove(activeRound);
                }

                AnnounceToPlayers(activeRound, isFinished);
            }
        }

        private void AnnounceToPlayers(ActiveRound activeRound, bool isFinished)
        {
            foreach (DuelPlayer player in activeRound.Players)
            {
                if (isFinished)
                {
                    _roundAnnouncer.AnnounceRoundFinished(player.Id, activeRound);
                }
                else
                {
                    _roundAnnouncer.AnnounceRoundChanged(player.Id, activeRound);
                }
            }
        }
    }
}
