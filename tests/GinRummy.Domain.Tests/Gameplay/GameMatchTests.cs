using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace GinRummy.Domain.Tests.Gameplay
{
    [TestFixture]
    public class GameMatchTests
    {
        private const int KnockerId = 1;
        private const int DefenderId = 2;
        private const int PointsOfAKnockAgainstLooseCards = 64;

        [Test]
        public void Test_ScoreFinishedRound_Knock_AddsThePointsToTheWinner()
        {
            GameMatch match = new GameMatch(BuildRoundWithKnockerAsNonDealer());
            KnockAsNonDealer(match.CurrentRound);

            match.ScoreFinishedRound();

            Assert.AreEqual(PointsOfAKnockAgainstLooseCards, match.GetScore(KnockerId));
        }

        [Test]
        public void Test_ScoreFinishedRound_WinnerReachesTheTarget_EndsTheMatchWithThatWinner()
        {
            GameMatch match = new GameMatch(BuildRoundWithKnockerAsNonDealer());
            KnockAsNonDealer(match.CurrentRound);
            match.ScoreFinishedRound();
            match.DealNextRound(BuildDeck(CreateDefenderHand(), CreateKnockerHand()));
            KnockAsDealer(match.CurrentRound);

            match.ScoreFinishedRound();

            Assert.AreEqual(KnockerId, match.WinnerId);
        }

        [Test]
        public void Test_ScoreFinishedRound_VoidRound_LeavesTheScoreAtZero()
        {
            GameMatch match = new GameMatch(new GameRound(DefenderId, KnockerId, StandardDeck.CreateOrdered()));
            PlayUntilTheStockRunsOut(match.CurrentRound);

            match.ScoreFinishedRound();

            Assert.AreEqual(0, match.GetScore(KnockerId) + match.GetScore(DefenderId));
        }

        [Test]
        public void Test_ScoreFinishedRound_RoundInProgress_ThrowsInvalidMoveException()
        {
            GameMatch match = new GameMatch(new GameRound(DefenderId, KnockerId, StandardDeck.CreateOrdered()));

            TestDelegate scoreFinishedRound = () => match.ScoreFinishedRound();

            Assert.Throws<InvalidMoveException>(scoreFinishedRound);
        }

        [Test]
        public void Test_DealNextRound_AfterScoring_SwapsTheDealer()
        {
            GameMatch match = new GameMatch(new GameRound(DefenderId, KnockerId, StandardDeck.CreateOrdered()));
            PlayUntilTheStockRunsOut(match.CurrentRound);
            match.ScoreFinishedRound();

            match.DealNextRound(StandardDeck.CreateOrdered());

            Assert.AreEqual(KnockerId, match.CurrentRound.DealerId);
        }

        [Test]
        public void Test_DealNextRound_RoundNotScored_ThrowsInvalidMoveException()
        {
            GameMatch match = new GameMatch(new GameRound(DefenderId, KnockerId, StandardDeck.CreateOrdered()));

            TestDelegate dealNextRound = () => match.DealNextRound(StandardDeck.CreateOrdered());

            Assert.Throws<InvalidMoveException>(dealNextRound);
        }

        [Test]
        public void Test_Constructor_ResumedScores_KeepsTheScoreOfEachPlayer()
        {
            Dictionary<int, int> scoresByPlayerId = new Dictionary<int, int> { { KnockerId, 40 }, { DefenderId, 70 } };

            GameMatch match = new GameMatch(new GameRound(DefenderId, KnockerId, StandardDeck.CreateOrdered()), scoresByPlayerId);

            Assert.AreEqual(70, match.GetScore(DefenderId));
        }

        [Test]
        public void Test_Constructor_ResumedScoreAtTheTarget_ThrowsArgumentException()
        {
            Dictionary<int, int> scoresByPlayerId = new Dictionary<int, int> { { KnockerId, RoundRules.TargetScore }, { DefenderId, 0 } };
            GameRound round = new GameRound(DefenderId, KnockerId, StandardDeck.CreateOrdered());

            TestDelegate resumeMatch = () => new GameMatch(round, scoresByPlayerId);

            Assert.Throws<ArgumentException>(resumeMatch);
        }

        [Test]
        public void Test_Constructor_ResumedScoresWithoutAPlayer_ThrowsArgumentException()
        {
            Dictionary<int, int> scoresByPlayerId = new Dictionary<int, int> { { KnockerId, 10 } };
            GameRound round = new GameRound(DefenderId, KnockerId, StandardDeck.CreateOrdered());

            TestDelegate resumeMatch = () => new GameMatch(round, scoresByPlayerId);

            Assert.Throws<ArgumentException>(resumeMatch);
        }

        private static GameRound BuildRoundWithKnockerAsNonDealer()
        {
            return new GameRound(DefenderId, KnockerId, BuildDeck(CreateKnockerHand(), CreateDefenderHand()));
        }

        private static IList<Card> BuildDeck(IList<Card> nonDealerHand, IList<Card> dealerHand)
        {
            List<Card> deck = new List<Card>();
            for (int index = 0; index < nonDealerHand.Count; index++)
            {
                deck.Add(nonDealerHand[index]);
                deck.Add(dealerHand[index]);
            }

            deck.Add(new Card(CardSuit.Diamonds, 2));
            deck.AddRange(StandardDeck.CreateOrdered().Where(card => !deck.Contains(card)));

            return deck;
        }

        private static void KnockAsNonDealer(GameRound round)
        {
            round.TakeFirstCard(KnockerId);
            Knock(round);
        }

        private static void KnockAsDealer(GameRound round)
        {
            round.PassFirstCard(DefenderId);
            round.TakeFirstCard(KnockerId);
            Knock(round);
        }

        private static void Knock(GameRound round)
        {
            List<Card> remainingCards = round.GetHand(KnockerId).Where(card => card.Rank != Card.KingRank).ToList();
            round.Knock(KnockerId, new Card(CardSuit.Diamonds, Card.KingRank), MeldFinder.FindBestMelds(remainingCards));
            round.LayDownDefenderHand(DefenderId, new List<IList<Card>>());
        }

        private static void PlayUntilTheStockRunsOut(GameRound round)
        {
            round.PassFirstCard(round.CurrentPlayerId);
            round.PassFirstCard(round.CurrentPlayerId);
            while (round.Phase != TurnPhase.Finished)
            {
                int playerId = round.CurrentPlayerId;
                round.DrawFromStock(playerId);
                round.Discard(playerId, round.GetHand(playerId).Last());
            }
        }

        private static IList<Card> CreateKnockerHand()
        {
            return new List<Card>
            {
                new Card(CardSuit.Hearts, Card.AceRank),
                new Card(CardSuit.Hearts, 2),
                new Card(CardSuit.Hearts, 3),
                new Card(CardSuit.Spades, 4),
                new Card(CardSuit.Spades, 5),
                new Card(CardSuit.Spades, 6),
                new Card(CardSuit.Clubs, 7),
                new Card(CardSuit.Clubs, 8),
                new Card(CardSuit.Clubs, 9),
                new Card(CardSuit.Diamonds, 13)
            };
        }

        private static IList<Card> CreateDefenderHand()
        {
            return new List<Card>
            {
                new Card(CardSuit.Diamonds, Card.AceRank),
                new Card(CardSuit.Diamonds, 3),
                new Card(CardSuit.Diamonds, 5),
                new Card(CardSuit.Diamonds, 7),
                new Card(CardSuit.Diamonds, 9),
                new Card(CardSuit.Hearts, 5),
                new Card(CardSuit.Hearts, 7),
                new Card(CardSuit.Hearts, 9),
                new Card(CardSuit.Spades, 11),
                new Card(CardSuit.Spades, 13)
            };
        }
    }
}
