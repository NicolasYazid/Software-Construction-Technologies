using System;

using GinRummy.Application.Ports.Gameplay;
using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Domain;

namespace GinRummy.Application.UseCases.Gameplay
{
    public class PlayRoundUseCase : IPlayRoundUseCase
    {
        private readonly MatchRegistry _matchRegistry;
        private readonly IRoundAnnouncer _roundAnnouncer;
        private readonly IMatchRecorder _matchRecorder;

        public PlayRoundUseCase(MatchRegistry matchRegistry, IRoundAnnouncer roundAnnouncer, IMatchRecorder matchRecorder)
        {
            _matchRegistry = matchRegistry ?? throw new ArgumentNullException(nameof(matchRegistry));
            _roundAnnouncer = roundAnnouncer ?? throw new ArgumentNullException(nameof(roundAnnouncer));
            _matchRecorder = matchRecorder ?? throw new ArgumentNullException(nameof(matchRecorder));
        }

        public void TakeFirstCard(int playerId)
        {
            ApplyMove(playerId, activeMatch => activeMatch.TakeFirstCard(playerId));
        }

        public void PassFirstCard(int playerId)
        {
            ApplyMove(playerId, activeMatch => activeMatch.PassFirstCard(playerId));
        }

        public void DrawFromStock(int playerId)
        {
            ApplyMove(playerId, activeMatch => activeMatch.DrawFromStock(playerId));
        }

        public void DrawFromDiscardPile(int playerId)
        {
            ApplyMove(playerId, activeMatch => activeMatch.DrawFromDiscardPile(playerId));
        }

        public void Discard(int playerId, Card card)
        {
            ApplyMove(playerId, activeMatch => activeMatch.Discard(playerId, card));
        }

        public void Knock(int playerId, Card discardedCard)
        {
            ApplyMove(playerId, activeMatch => activeMatch.Knock(playerId, discardedCard));
        }

        public void ForfeitRound(int playerId)
        {
            ActiveMatch activeMatch = _matchRegistry.Find(playerId);
            if (activeMatch == null)
            {
                return;
            }

            lock (activeMatch.SyncRoot)
            {
                if (_matchRegistry.Remove(activeMatch))
                {
                    int winnerId = activeMatch.FindOpponent(playerId).Id;
                    _roundAnnouncer.AnnounceOpponentLeft(winnerId);
                    _matchRecorder.RecordEnd(activeMatch, winnerId);
                }
            }
        }

        private void ApplyMove(int playerId, Action<ActiveMatch> play)
        {
            ActiveMatch activeMatch = _matchRegistry.Find(playerId);
            if (activeMatch == null)
            {
                throw new InvalidMoveException("The player is not playing a round.");
            }

            lock (activeMatch.SyncRoot)
            {
                play(activeMatch);
                if (activeMatch.Round.Phase == TurnPhase.Finished)
                {
                    FinishRound(activeMatch);
                }
                else
                {
                    AnnounceToPlayers(activeMatch, _roundAnnouncer.AnnounceRoundChanged);
                }
            }
        }

        private void FinishRound(ActiveMatch activeMatch)
        {
            activeMatch.Match.ScoreFinishedRound();
            AnnounceToPlayers(activeMatch, _roundAnnouncer.AnnounceRoundFinished);
            if (activeMatch.Match.IsOver)
            {
                _matchRegistry.Remove(activeMatch);
                _matchRecorder.RecordEnd(activeMatch, activeMatch.Match.WinnerId.Value);
            }
            else
            {
                _matchRecorder.RecordRoundScores(activeMatch);
                _matchRegistry.DealNextRound(activeMatch);
                AnnounceToPlayers(activeMatch, _roundAnnouncer.AnnounceRoundChanged);
            }
        }

        private static void AnnounceToPlayers(ActiveMatch activeMatch, Action<int, ActiveMatch> announce)
        {
            foreach (DuelPlayer player in activeMatch.Players)
            {
                announce(player.Id, activeMatch);
            }
        }
    }
}
