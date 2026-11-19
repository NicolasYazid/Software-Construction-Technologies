using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GinRummy.Client.Models;
using NUnit.Framework;

namespace GinRummy.Client.Tests.Models
{
    [TestFixture]
    public class HandOrderTests
    {
        private static readonly Card s_kingOfClubs = new Card(CardRank.King, CardSuit.Clubs);
        private static readonly Card s_fiveOfHearts = new Card(CardRank.Five, CardSuit.Hearts);
        private static readonly Card s_fiveOfClubs = new Card(CardRank.Five, CardSuit.Clubs);
        private static readonly Card s_aceOfSpades = new Card(CardRank.Ace, CardSuit.Spades);

        [Test]
        public void Test_SortByRank_MixedHand_OrdersFromAceToKing()
        {
            List<Card> hand = new List<Card> { s_kingOfClubs, s_fiveOfHearts, s_aceOfSpades, s_fiveOfClubs };

            IList<Card> sortedHand = HandOrder.SortByRank(hand);

            CollectionAssert.AreEqual(new[] { s_aceOfSpades, s_fiveOfClubs, s_fiveOfHearts, s_kingOfClubs }, sortedHand);
        }

        [Test]
        public void Test_PutMeldsFirst_LooseCards_KeepPlayerOrder()
        {
            List<Card> hand = new List<Card> { s_kingOfClubs, s_fiveOfHearts, s_aceOfSpades, s_fiveOfClubs };
            List<Card> meldCards = new List<Card> { s_fiveOfClubs, s_fiveOfHearts };

            IList<Card> groupedHand = HandOrder.PutMeldsFirst(hand, meldCards);

            CollectionAssert.AreEqual(new[] { s_fiveOfClubs, s_fiveOfHearts, s_kingOfClubs, s_aceOfSpades }, groupedHand);
        }

        [Test]
        public void Test_Arrange_ShuffledHand_MatchesOrder()
        {
            ObservableCollection<Card> hand = new ObservableCollection<Card> { s_kingOfClubs, s_fiveOfHearts, s_aceOfSpades, s_fiveOfClubs };
            List<Card> order = new List<Card> { s_aceOfSpades, s_fiveOfClubs, s_fiveOfHearts, s_kingOfClubs };

            HandOrder.Arrange(hand, order);

            CollectionAssert.AreEqual(order, hand.ToList());
        }

        [Test]
        public void Test_SplitIntoNeighbors_CardApart_StartsNewStretch()
        {
            List<Card> hand = new List<Card> { s_fiveOfClubs, s_fiveOfHearts, s_kingOfClubs, s_aceOfSpades };

            IList<IList<Card>> stretches = HandOrder.SplitIntoNeighbors(hand, new[] { s_aceOfSpades, s_fiveOfHearts, s_fiveOfClubs });

            Assert.AreEqual(2, stretches.Count);
            CollectionAssert.AreEqual(new[] { s_fiveOfClubs, s_fiveOfHearts }, stretches[0]);
            CollectionAssert.AreEqual(new[] { s_aceOfSpades }, stretches[1]);
        }

        [Test]
        public void Test_SortLooseCards_HandWithMeld_KeepsMeldFirstInPlayerOrder()
        {
            List<Card> hand = new List<Card> { s_kingOfClubs, s_fiveOfHearts, s_aceOfSpades, s_fiveOfClubs };
            List<IList<Card>> melds = new List<IList<Card>> { new List<Card> { s_fiveOfClubs, s_fiveOfHearts } };

            IList<Card> sortedHand = HandOrder.SortLooseCards(hand, melds);

            CollectionAssert.AreEqual(new[] { s_fiveOfHearts, s_fiveOfClubs, s_aceOfSpades, s_kingOfClubs }, sortedHand);
        }
    }
}