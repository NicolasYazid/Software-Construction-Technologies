using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Domain;
using NUnit.Framework;

namespace GinRummy.Application.Tests.Gameplay
{
    [TestFixture]
    public class ActiveMatchTests
    {
        private const int NonDealerId = 1;
        private const int DealerId = 2;

        [Test]
        public void Test_Knock_DeadwoodWithinThreshold_FinishesTheRound()
        {
            ActiveMatch activeMatch = StartRound(new Card(CardSuit.Diamonds, 2));
            activeMatch.TakeFirstCard(NonDealerId);

            activeMatch.Knock(NonDealerId, new Card(CardSuit.Diamonds, 13));

            Assert.AreEqual(TurnPhase.Finished, activeMatch.Round.Phase);
        }

        [Test]
        public void Test_Knock_DeadwoodAboveThreshold_ThrowsInvalidMoveException()
        {
            ActiveMatch activeMatch = StartRound(new Card(CardSuit.Diamonds, 11));
            activeMatch.TakeFirstCard(NonDealerId);

            TestDelegate knock = () => activeMatch.Knock(NonDealerId, new Card(CardSuit.Hearts, Card.AceRank));

            Assert.Throws<InvalidMoveException>(knock);
        }

        [Test]
        public void Test_FindKnockDiscards_CardTakenFromDiscardPile_LeavesItOut()
        {
            ActiveMatch activeMatch = StartRound(new Card(CardSuit.Diamonds, 2));
            activeMatch.TakeFirstCard(NonDealerId);

            IList<Card> discards = activeMatch.FindKnockDiscards(NonDealerId);

            CollectionAssert.AreEquivalent(new[] { new Card(CardSuit.Diamonds, 13) }, discards);
        }

        [Test]
        public void Test_FindKnockDiscards_FirstCardOffer_ReturnsEmpty()
        {
            ActiveMatch activeMatch = StartRound(new Card(CardSuit.Diamonds, 2));

            IList<Card> discards = activeMatch.FindKnockDiscards(NonDealerId);

            Assert.IsEmpty(discards);
        }

        [Test]
        public void Test_DealNextRound_AfterAScoredKnock_RecordsTheDealOfTheNewDealer()
        {
            ActiveMatch activeMatch = StartRound(new Card(CardSuit.Diamonds, 2));
            activeMatch.TakeFirstCard(NonDealerId);
            activeMatch.Knock(NonDealerId, new Card(CardSuit.Diamonds, 13));
            activeMatch.Match.ScoreFinishedRound();

            activeMatch.DealNextRound(StandardDeck.CreateOrdered());

            Assert.AreEqual(NonDealerId, activeMatch.LastMoverId);
        }

        private static ActiveMatch StartRound(Card faceUpCard)
        {
            IList<Card> nonDealerHand = new List<Card>
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
            IList<Card> dealerHand = new List<Card>
            {
                new Card(CardSuit.Diamonds, Card.AceRank),
                new Card(CardSuit.Diamonds, 3),
                new Card(CardSuit.Diamonds, 5),
                new Card(CardSuit.Diamonds, 7),
                new Card(CardSuit.Diamonds, 9),
                new Card(CardSuit.Hearts, 5),
                new Card(CardSuit.Hearts, 7),
                new Card(CardSuit.Hearts, 9),
                new Card(CardSuit.Spades, 12),
                new Card(CardSuit.Spades, 13)
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
                new DuelPlayer(DealerId, "Dealer", 1002),
                new DuelPlayer(NonDealerId, "Non-dealer", 1001));
        }
    }
}
