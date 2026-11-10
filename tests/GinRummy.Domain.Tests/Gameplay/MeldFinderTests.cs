using System.Collections.Generic;

using GinRummy.Domain.Gameplay;
using NUnit.Framework;

namespace GinRummy.Domain.Tests.Gameplay
{
    [TestFixture]
    public class MeldFinderTests
    {
        [Test]
        public void Test_CountDeadwood_TwoRunsAndSetWithLooseKing_ReturnsTen()
        {
            IList<Card> hand = new List<Card>
            {
                new Card(CardSuit.Hearts, 1), new Card(CardSuit.Hearts, 2), new Card(CardSuit.Hearts, 3),
                new Card(CardSuit.Spades, 4), new Card(CardSuit.Spades, 5), new Card(CardSuit.Spades, 6),
                new Card(CardSuit.Clubs, 9), new Card(CardSuit.Diamonds, 9), new Card(CardSuit.Spades, 9),
                new Card(CardSuit.Diamonds, 13)
            };

            int deadwood = MeldFinder.CountDeadwood(hand);

            Assert.AreEqual(10, deadwood);
        }

        [Test]
        public void Test_CountDeadwood_CardFitsRunAndSet_ReturnsZero()
        {
            IList<Card> hand = new List<Card>
            {
                new Card(CardSuit.Hearts, 4), new Card(CardSuit.Hearts, 5), new Card(CardSuit.Hearts, 6),
                new Card(CardSuit.Hearts, 7), new Card(CardSuit.Spades, 7), new Card(CardSuit.Clubs, 7)
            };

            int deadwood = MeldFinder.CountDeadwood(hand);

            Assert.AreEqual(0, deadwood);
        }

        [Test]
        public void Test_CountDeadwood_NoMelds_ReturnsSumOfCards()
        {
            IList<Card> hand = new List<Card>
            {
                new Card(CardSuit.Hearts, 2), new Card(CardSuit.Spades, 5), new Card(CardSuit.Clubs, 13)
            };

            int deadwood = MeldFinder.CountDeadwood(hand);

            Assert.AreEqual(17, deadwood);
        }

        [Test]
        public void Test_FindBestMelds_FourCardsOfSameRank_ReturnsOneMeld()
        {
            IList<Card> nines = new List<Card>
            {
                new Card(CardSuit.Hearts, 9), new Card(CardSuit.Spades, 9), new Card(CardSuit.Clubs, 9),
                new Card(CardSuit.Diamonds, 9)
            };

            IList<IList<Card>> melds = MeldFinder.FindBestMelds(nines);

            Assert.AreEqual(1, melds.Count);
        }
    }
}
