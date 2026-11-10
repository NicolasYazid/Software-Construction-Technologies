using System.Collections.Generic;

using NUnit.Framework;

namespace GinRummy.Domain.Tests.Gameplay
{
    [TestFixture]
    public class MeldRulesTests
    {
        [Test]
        public void Test_IsValidMeld_ThreeCardsOfSameRank_ReturnsTrue()
        {
            IList<Card> sevens = new List<Card>
            {
                new Card(CardSuit.Hearts, 7), new Card(CardSuit.Spades, 7), new Card(CardSuit.Clubs, 7)
            };

            bool isValid = MeldRules.IsValidMeld(sevens);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void Test_IsValidMeld_FourCardsOfSameRank_ReturnsTrue()
        {
            IList<Card> kings = new List<Card>
            {
                new Card(CardSuit.Hearts, 13), new Card(CardSuit.Spades, 13), new Card(CardSuit.Clubs, 13),
                new Card(CardSuit.Diamonds, 13)
            };

            bool isValid = MeldRules.IsValidMeld(kings);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void Test_IsValidMeld_RunOfSameSuitInAnyOrder_ReturnsTrue()
        {
            IList<Card> run = new List<Card>
            {
                new Card(CardSuit.Clubs, 6), new Card(CardSuit.Clubs, 4), new Card(CardSuit.Clubs, 5),
                new Card(CardSuit.Clubs, 7)
            };

            bool isValid = MeldRules.IsValidMeld(run);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void Test_IsValidMeld_RunWithMixedSuits_ReturnsFalse()
        {
            IList<Card> mixedRun = new List<Card>
            {
                new Card(CardSuit.Clubs, 4), new Card(CardSuit.Hearts, 5), new Card(CardSuit.Clubs, 6)
            };

            bool isValid = MeldRules.IsValidMeld(mixedRun);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void Test_IsValidMeld_RunWithGap_ReturnsFalse()
        {
            IList<Card> runWithGap = new List<Card>
            {
                new Card(CardSuit.Diamonds, 2), new Card(CardSuit.Diamonds, 3), new Card(CardSuit.Diamonds, 5)
            };

            bool isValid = MeldRules.IsValidMeld(runWithGap);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void Test_IsValidMeld_AceTwoThree_ReturnsTrue()
        {
            IList<Card> lowRun = new List<Card>
            {
                new Card(CardSuit.Hearts, Card.AceRank), new Card(CardSuit.Hearts, 2), new Card(CardSuit.Hearts, 3)
            };

            bool isValid = MeldRules.IsValidMeld(lowRun);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void Test_IsValidMeld_QueenKingAce_ReturnsFalse()
        {
            IList<Card> highRun = new List<Card>
            {
                new Card(CardSuit.Hearts, 12), new Card(CardSuit.Hearts, Card.KingRank), new Card(CardSuit.Hearts, Card.AceRank)
            };

            bool isValid = MeldRules.IsValidMeld(highRun);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void Test_IsValidMeld_TwoCards_ReturnsFalse()
        {
            IList<Card> pair = new List<Card> { new Card(CardSuit.Hearts, 9), new Card(CardSuit.Spades, 9) };

            bool isValid = MeldRules.IsValidMeld(pair);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void Test_IsValidMeld_SameCardTwice_ReturnsFalse()
        {
            IList<Card> repeatedCard = new List<Card>
            {
                new Card(CardSuit.Spades, 9), new Card(CardSuit.Spades, 9), new Card(CardSuit.Hearts, 9)
            };

            bool isValid = MeldRules.IsValidMeld(repeatedCard);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void Test_CanLayOff_CardThatExtendsRun_ReturnsTrue()
        {
            IList<Card> run = new List<Card>
            {
                new Card(CardSuit.Clubs, 7), new Card(CardSuit.Clubs, 8), new Card(CardSuit.Clubs, 9)
            };

            bool canLayOff = MeldRules.CanLayOff(new Card(CardSuit.Clubs, 10), run);

            Assert.IsTrue(canLayOff);
        }

        [Test]
        public void Test_CanLayOff_CardOfAnotherSuit_ReturnsFalse()
        {
            IList<Card> run = new List<Card>
            {
                new Card(CardSuit.Clubs, 7), new Card(CardSuit.Clubs, 8), new Card(CardSuit.Clubs, 9)
            };

            bool canLayOff = MeldRules.CanLayOff(new Card(CardSuit.Hearts, 10), run);

            Assert.IsFalse(canLayOff);
        }
    }
}
