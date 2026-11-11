using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Contracts.DataContracts.Gameplay;
using GinRummy.Domain;
using ContractCardSuit = GinRummy.Contracts.DataContracts.Gameplay.CardSuit;
using DomainCardSuit = GinRummy.Domain.CardSuit;

namespace GinRummy.Services.Mappers.Gameplay
{
    public static class GameMapper
    {
        public static Card MapToCard(PlayingCard playingCard)
        {
            return new Card(MapByName<DomainCardSuit>(playingCard.Suit), playingCard.Rank);
        }

        public static RoundDetails MapToRoundDetails(ActiveRound activeRound, int viewerId)
        {
            GameRound round = activeRound.Round;
            DuelPlayer opponent = activeRound.FindOpponent(viewerId);
            List<Card> hand = round.GetHand(viewerId).ToList();
            RoundDetails details = new RoundDetails
            {
                OpponentName = opponent.Name,
                IsYourTurn = round.CurrentPlayerId == viewerId,
                Step = MapToRoundStep(round.Phase),
                Hand = MapToPlayingCards(hand),
                OpponentCardCount = round.GetHand(opponent.Id).Count,
                StockCount = round.StockCount,
                TopOfDiscardPile = MapToPlayingCard(round.TopOfDiscardPile),
                Deadwood = MeldFinder.CountDeadwood(hand),
                LastMove = MapByName<RoundMoveKind>(activeRound.LastMove),
                LastMoverName = activeRound.FindPlayer(activeRound.LastMoverId).Name,
                LastMoveCard = MapToPlayingCard(activeRound.LastMoveCard)
            };

            return details;
        }

        public static RoundSummary MapToRoundSummary(ActiveRound activeRound, int viewerId)
        {
            RoundResult result = activeRound.Round.Result;
            RoundSummary summary = new RoundSummary
            {
                Ending = MapByName<RoundEnding>(result.Outcome),
                Points = result.Points,
                IsWinner = result.WinnerId == viewerId,
                WinnerName = FindPlayerName(activeRound, result.WinnerId),
                KnockerName = FindPlayerName(activeRound, activeRound.KnockerId),
                KnockerMelds = new List<List<PlayingCard>>(),
                KnockerUnmatchedCards = new List<PlayingCard>(),
                DefenderMelds = new List<List<PlayingCard>>(),
                DefenderUnmatchedCards = new List<PlayingCard>()
            };
            if (activeRound.KnockerId.HasValue)
            {
                AddHandsToSummary(summary, activeRound);
            }

            return summary;
        }

        private static void AddHandsToSummary(RoundSummary summary, ActiveRound activeRound)
        {
            int knockerId = activeRound.KnockerId.Value;
            int defenderId = activeRound.FindOpponent(knockerId).Id;
            List<Card> knockerHand = activeRound.Round.GetHand(knockerId).ToList();
            List<Card> defenderHand = activeRound.Round.GetHand(defenderId).ToList();
            IList<IList<Card>> knockerMelds = MeldFinder.FindBestMelds(knockerHand);
            IList<IList<Card>> defenderMelds = MeldFinder.FindBestMelds(defenderHand);
            summary.KnockerDeadwood = MeldFinder.CountDeadwood(knockerHand);
            summary.DefenderDeadwood = MeldFinder.CountDeadwood(defenderHand);
            summary.KnockerMelds = MapToMelds(knockerMelds);
            summary.KnockerUnmatchedCards = MapToUnmatchedCards(knockerHand, knockerMelds);
            summary.DefenderMelds = MapToMelds(defenderMelds);
            summary.DefenderUnmatchedCards = MapToUnmatchedCards(defenderHand, defenderMelds);
        }

        private static string FindPlayerName(ActiveRound activeRound, int? playerId)
        {
            string playerName = null;
            if (playerId.HasValue)
            {
                playerName = activeRound.FindPlayer(playerId.Value).Name;
            }

            return playerName;
        }

        private static RoundStep MapToRoundStep(TurnPhase phase)
        {
            RoundStep step;
            switch (phase)
            {
                case TurnPhase.Draw:
                    step = RoundStep.Drawing;
                    break;
                case TurnPhase.Discard:
                    step = RoundStep.Discarding;
                    break;
                default:
                    step = RoundStep.OfferingFirstCard;
                    break;
            }

            return step;
        }

        private static PlayingCard MapToPlayingCard(Card card)
        {
            PlayingCard playingCard = null;
            if (card != null)
            {
                playingCard = new PlayingCard { Suit = MapByName<ContractCardSuit>(card.Suit), Rank = card.Rank };
            }

            return playingCard;
        }

        private static List<PlayingCard> MapToPlayingCards(IEnumerable<Card> cards)
        {
            return cards.Select(card => MapToPlayingCard(card)).ToList();
        }

        private static List<List<PlayingCard>> MapToMelds(IList<IList<Card>> melds)
        {
            return melds.Select(meld => MapToPlayingCards(meld)).ToList();
        }

        private static List<PlayingCard> MapToUnmatchedCards(List<Card> hand, IList<IList<Card>> melds)
        {
            List<Card> meldedCards = melds.SelectMany(meld => meld).ToList();

            return MapToPlayingCards(hand.Where(card => !meldedCards.Contains(card)));
        }

        private static TTarget MapByName<TTarget>(object source)
        {
            return (TTarget)Enum.Parse(typeof(TTarget), source.ToString());
        }
    }
}
