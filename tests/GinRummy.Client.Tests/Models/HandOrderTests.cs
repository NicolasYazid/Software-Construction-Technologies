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
        private static readonly Card KingOfClubs = new Card(CardRank.King, CardSuit.Clubs);
        private static readonly Card FiveOfHearts = new Card(CardRank.Five, CardSuit.Hearts);
        private static readonly Card FiveOfClubs = new Card(CardRank.Five, CardSuit.Clubs);
        private static readonly Card AceOfSpades = new Card(CardRank.Ace, CardSuit.Spades);

        [Test]
        public void Test_SortByRank_MixedHand_OrdersFromAceToKing()
        {
            List<Card> hand = new List<Card> { KingOfClubs, FiveOfHearts, AceOfSpades, FiveOfClubs };

            IList<Card> sortedHand = HandOrder.SortByRank(hand);

            CollectionAssert.AreEqual(new[] { AceOfSpades, FiveOfClubs, FiveOfHearts, KingOfClubs }, sortedHand);
        }

        [Test]
        public void Test_PutMeldsFirst_LooseCards_KeepPlayerOrder()
        {
            List<Card> hand = new List<Card> { KingOfClubs, FiveOfHearts, AceOfSpades, FiveOfClubs };
            List<Card> meldCards = new List<Card> { FiveOfClubs, FiveOfHearts };

            IList<Card> groupedHand = HandOrder.PutMeldsFirst(hand, meldCards);

            CollectionAssert.AreEqual(new[] { FiveOfClubs, FiveOfHearts, KingOfClubs, AceOfSpades }, groupedHand);
        }

        [Test]
        public void Test_Arrange_ShuffledHand_MatchesOrder()
        {
            ObservableCollection<Card> hand = new ObservableCollection<Card> { KingOfClubs, FiveOfHearts, AceOfSpades, FiveOfClubs };
            List<Card> order = new List<Card> { AceOfSpades, FiveOfClubs, FiveOfHearts, KingOfClubs };

            HandOrder.Arrange(hand, order);

            CollectionAssert.AreEqual(order, hand.ToList());
        }
    }
}