using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Domain;
using NUnit.Framework;

namespace GinRummy.Server.Tests.Domain.Gameplay
{
    [TestFixture]
    public class GameMatchTests
    {
        private const int KnockerId = 1;
        private const int DefenderId = 2;
        private const int PointsOfAKnockAgainstLooseCards = 64;
        private const int KnockerResumedScore = 40;
        private const int DefenderResumedScore = 70;

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
            Dictionary<int, int> scoresByPlayerId = new Dictionary<int, int>
            {
                { KnockerId, KnockerResumedScore },
                { DefenderId, DefenderResumedScore }
            };

            GameMatch match = new GameMatch(new GameRound(DefenderId, KnockerId, StandardDeck.CreateOrdered()), scoresByPlayerId);

            Assert.AreEqual(DefenderResumedScore, match.GetScore(DefenderId));
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
            Dictionary<int, int> scoresByPlayerId = new Dictionary<int, int> { { KnockerId, KnockerResumedScore } };
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

            deck.Add(new Card(CardSuit.Diamonds, RankValues.Two));
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
                new Card(CardSuit.Hearts, RankValues.Two),
                new Card(CardSuit.Hearts, RankValues.Three),
                new Card(CardSuit.Spades, RankValues.Four),
                new Card(CardSuit.Spades, RankValues.Five),
                new Card(CardSuit.Spades, RankValues.Six),
                new Card(CardSuit.Clubs, RankValues.Seven),
                new Card(CardSuit.Clubs, RankValues.Eight),
                new Card(CardSuit.Clubs, RankValues.Nine),
                new Card(CardSuit.Diamonds, Card.KingRank)
            };
        }

        private static IList<Card> CreateDefenderHand()
        {
            return new List<Card>
            {
                new Card(CardSuit.Diamonds, Card.AceRank),
                new Card(CardSuit.Diamonds, RankValues.Three),
                new Card(CardSuit.Diamonds, RankValues.Five),
                new Card(CardSuit.Diamonds, RankValues.Seven),
                new Card(CardSuit.Diamonds, RankValues.Nine),
                new Card(CardSuit.Hearts, RankValues.Five),
                new Card(CardSuit.Hearts, RankValues.Seven),
                new Card(CardSuit.Hearts, RankValues.Nine),
                new Card(CardSuit.Spades, RankValues.Jack),
                new Card(CardSuit.Spades, Card.KingRank)
            };
        }
    }
}
