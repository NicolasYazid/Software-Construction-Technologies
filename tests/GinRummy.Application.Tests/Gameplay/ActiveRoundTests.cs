using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Domain.Gameplay;
using NUnit.Framework;

namespace GinRummy.Application.Tests.Gameplay
{
    [TestFixture]
    public class ActiveRoundTests
    {
        private const int NonDealerId = 1;
        private const int DealerId = 2;

        [Test]
        public void Test_Knock_DeadwoodWithinThreshold_FinishesTheRound()
        {
            ActiveRound activeRound = StartRound(new Card(CardSuit.Diamonds, 2));
            activeRound.TakeFirstCard(NonDealerId);

            activeRound.Knock(NonDealerId, new Card(CardSuit.Diamonds, 13));

            Assert.AreEqual(TurnPhase.Finished, activeRound.Round.Phase);
        }

        [Test]
        public void Test_Knock_DeadwoodAboveThreshold_ThrowsInvalidMoveException()
        {
            ActiveRound activeRound = StartRound(new Card(CardSuit.Diamonds, 11));
            activeRound.TakeFirstCard(NonDealerId);

            TestDelegate knock = () => activeRound.Knock(NonDealerId, new Card(CardSuit.Hearts, Card.AceRank));

            Assert.Throws<InvalidMoveException>(knock);
        }

        // The non-dealer holds three runs and a loose king, so the face-up card decides how much deadwood is left.
        private static ActiveRound StartRound(Card faceUpCard)
        {
            IList<Card> nonDealerHand = new List<Card>
            {
                new Card(CardSuit.Hearts, Card.AceRank), new Card(CardSuit.Hearts, 2), new Card(CardSuit.Hearts, 3),
                new Card(CardSuit.Spades, 4), new Card(CardSuit.Spades, 5), new Card(CardSuit.Spades, 6),
                new Card(CardSuit.Clubs, 7), new Card(CardSuit.Clubs, 8), new Card(CardSuit.Clubs, 9),
                new Card(CardSuit.Diamonds, 13)
            };
            IList<Card> dealerHand = new List<Card>
            {
                new Card(CardSuit.Diamonds, Card.AceRank), new Card(CardSuit.Diamonds, 3), new Card(CardSuit.Diamonds, 5),
                new Card(CardSuit.Diamonds, 7), new Card(CardSuit.Diamonds, 9), new Card(CardSuit.Hearts, 5),
                new Card(CardSuit.Hearts, 7), new Card(CardSuit.Hearts, 9), new Card(CardSuit.Spades, 12),
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

            return new ActiveRound(round, new DuelPlayer(DealerId, "Dealer", 1002), new DuelPlayer(NonDealerId, "Non-dealer", 1001));
        }
    }
}
