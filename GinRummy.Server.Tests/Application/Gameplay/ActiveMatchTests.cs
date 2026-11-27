using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Domain;
using NUnit.Framework;

namespace GinRummy.Server.Tests.Application.Gameplay
{
    [TestFixture]
    public class ActiveMatchTests
    {
        private const int NonDealerId = 1;
        private const int DealerId = 2;
        private const int NonDealerPublicTag = 1001;
        private const int DealerPublicTag = 1002;

        [Test]
        public void Test_Knock_DeadwoodWithinThreshold_FinishesTheRound()
        {
            ActiveMatch activeMatch = StartRound(new Card(CardSuit.Diamonds, RankValues.Two));
            activeMatch.TakeFirstCard(NonDealerId);

            activeMatch.Knock(NonDealerId, new Card(CardSuit.Diamonds, Card.KingRank));

            Assert.AreEqual(TurnPhase.Finished, activeMatch.Round.Phase);
        }

        [Test]
        public void Test_Knock_DeadwoodAboveThreshold_ThrowsInvalidMoveException()
        {
            ActiveMatch activeMatch = StartRound(new Card(CardSuit.Diamonds, RankValues.Jack));
            activeMatch.TakeFirstCard(NonDealerId);

            TestDelegate knock = () => activeMatch.Knock(NonDealerId, new Card(CardSuit.Hearts, Card.AceRank));

            Assert.Throws<InvalidMoveException>(knock);
        }

        [Test]
        public void Test_FindKnockDiscards_CardTakenFromDiscardPile_LeavesItOut()
        {
            ActiveMatch activeMatch = StartRound(new Card(CardSuit.Diamonds, RankValues.Two));
            activeMatch.TakeFirstCard(NonDealerId);

            IList<Card> discards = activeMatch.FindKnockDiscards(NonDealerId);

            CollectionAssert.AreEquivalent(new[] { new Card(CardSuit.Diamonds, Card.KingRank) }, discards);
        }

        [Test]
        public void Test_FindKnockDiscards_FirstCardOffer_ReturnsEmpty()
        {
            ActiveMatch activeMatch = StartRound(new Card(CardSuit.Diamonds, RankValues.Two));

            IList<Card> discards = activeMatch.FindKnockDiscards(NonDealerId);

            Assert.IsEmpty(discards);
        }

        [Test]
        public void Test_DealNextRound_AfterAScoredKnock_RecordsTheDealOfTheNewDealer()
        {
            ActiveMatch activeMatch = StartRound(new Card(CardSuit.Diamonds, RankValues.Two));
            activeMatch.TakeFirstCard(NonDealerId);
            activeMatch.Knock(NonDealerId, new Card(CardSuit.Diamonds, Card.KingRank));
            activeMatch.Match.ScoreFinishedRound();

            activeMatch.DealNextRound(StandardDeck.CreateOrdered());

            Assert.AreEqual(NonDealerId, activeMatch.LastMoverId);
        }

        private static ActiveMatch StartRound(Card faceUpCard)
        {
            IList<Card> nonDealerHand = new List<Card>
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
            IList<Card> dealerHand = new List<Card>
            {
                new Card(CardSuit.Diamonds, Card.AceRank),
                new Card(CardSuit.Diamonds, RankValues.Three),
                new Card(CardSuit.Diamonds, RankValues.Five),
                new Card(CardSuit.Diamonds, RankValues.Seven),
                new Card(CardSuit.Diamonds, RankValues.Nine),
                new Card(CardSuit.Hearts, RankValues.Five),
                new Card(CardSuit.Hearts, RankValues.Seven),
                new Card(CardSuit.Hearts, RankValues.Nine),
                new Card(CardSuit.Spades, RankValues.Queen),
                new Card(CardSuit.Spades, Card.KingRank)
            };
            List<Card> deck = new List<Card>();
            for (int index = 0; index < nonDealerHand.Count; index++)
            {
                deck.Add(nonDealerHand[index]);
                deck.Add(dealerHand[index]);
            }

            deck.Add(faceUpCard);
            deck.AddRange(StandardDeck.CreateOrdered().Where(card => !deck.Contains(card)));
            GameRound round = new GameRound(DealerId, NonDealerId, deck);

            return new ActiveMatch(
                new GameMatch(round),
                new DuelPlayer(DealerId, "Dealer", DealerPublicTag),
                new DuelPlayer(NonDealerId, "Non-dealer", NonDealerPublicTag));
        }
    }
}
