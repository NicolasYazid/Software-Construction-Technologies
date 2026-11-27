using System.Collections.Generic;

using GinRummy.Domain;
using NUnit.Framework;

namespace GinRummy.Server.Tests.Domain.Gameplay
{
    [TestFixture]
    public class MeldRulesTests
    {
        [Test]
        public void Test_IsValidMeld_ThreeCardsOfSameRank_ReturnsTrue()
        {
            IList<Card> sevens = new List<Card>
            {
                new Card(CardSuit.Hearts, RankValues.Seven),
                new Card(CardSuit.Spades, RankValues.Seven),
                new Card(CardSuit.Clubs, RankValues.Seven)
            };

            bool isValid = MeldRules.IsValidMeld(sevens);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void Test_IsValidMeld_FourCardsOfSameRank_ReturnsTrue()
        {
            IList<Card> kings = new List<Card>
            {
                new Card(CardSuit.Hearts, Card.KingRank),
                new Card(CardSuit.Spades, Card.KingRank),
                new Card(CardSuit.Clubs, Card.KingRank),
                new Card(CardSuit.Diamonds, Card.KingRank)
            };

            bool isValid = MeldRules.IsValidMeld(kings);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void Test_IsValidMeld_RunOfSameSuitInAnyOrder_ReturnsTrue()
        {
            IList<Card> run = new List<Card>
            {
                new Card(CardSuit.Clubs, RankValues.Six),
                new Card(CardSuit.Clubs, RankValues.Four),
                new Card(CardSuit.Clubs, RankValues.Five),
                new Card(CardSuit.Clubs, RankValues.Seven)
            };

            bool isValid = MeldRules.IsValidMeld(run);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void Test_IsValidMeld_RunWithMixedSuits_ReturnsFalse()
        {
            IList<Card> mixedRun = new List<Card>
            {
                new Card(CardSuit.Clubs, RankValues.Four),
                new Card(CardSuit.Hearts, RankValues.Five),
                new Card(CardSuit.Clubs, RankValues.Six)
            };

            bool isValid = MeldRules.IsValidMeld(mixedRun);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void Test_IsValidMeld_RunWithGap_ReturnsFalse()
        {
            IList<Card> runWithGap = new List<Card>
            {
                new Card(CardSuit.Diamonds, RankValues.Two),
                new Card(CardSuit.Diamonds, RankValues.Three),
                new Card(CardSuit.Diamonds, RankValues.Five)
            };

            bool isValid = MeldRules.IsValidMeld(runWithGap);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void Test_IsValidMeld_AceTwoThree_ReturnsTrue()
        {
            IList<Card> lowRun = new List<Card>
            {
                new Card(CardSuit.Hearts, Card.AceRank),
                new Card(CardSuit.Hearts, RankValues.Two),
                new Card(CardSuit.Hearts, RankValues.Three)
            };

            bool isValid = MeldRules.IsValidMeld(lowRun);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void Test_IsValidMeld_QueenKingAce_ReturnsFalse()
        {
            IList<Card> highRun = new List<Card>
            {
                new Card(CardSuit.Hearts, RankValues.Queen),
                new Card(CardSuit.Hearts, Card.KingRank),
                new Card(CardSuit.Hearts, Card.AceRank)
            };

            bool isValid = MeldRules.IsValidMeld(highRun);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void Test_IsValidMeld_TwoCards_ReturnsFalse()
        {
            IList<Card> pair = new List<Card>
            {
                new Card(CardSuit.Hearts, RankValues.Nine),
                new Card(CardSuit.Spades, RankValues.Nine)
            };

            bool isValid = MeldRules.IsValidMeld(pair);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void Test_IsValidMeld_SameCardTwice_ReturnsFalse()
        {
            IList<Card> repeatedCards = new List<Card>
            {
                new Card(CardSuit.Spades, RankValues.Nine),
                new Card(CardSuit.Spades, RankValues.Nine),
                new Card(CardSuit.Hearts, RankValues.Nine)
            };

            bool isValid = MeldRules.IsValidMeld(repeatedCards);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void Test_CanLayOff_CardThatExtendsRun_ReturnsTrue()
        {
            IList<Card> run = new List<Card>
            {
                new Card(CardSuit.Clubs, RankValues.Seven),
                new Card(CardSuit.Clubs, RankValues.Eight),
                new Card(CardSuit.Clubs, RankValues.Nine)
            };

            bool canLayOff = MeldRules.CanLayOff(new Card(CardSuit.Clubs, RankValues.Ten), run);

            Assert.IsTrue(canLayOff);
        }

        [Test]
        public void Test_CanLayOff_CardOfAnotherSuit_ReturnsFalse()
        {
            IList<Card> run = new List<Card>
            {
                new Card(CardSuit.Clubs, RankValues.Seven),
                new Card(CardSuit.Clubs, RankValues.Eight),
                new Card(CardSuit.Clubs, RankValues.Nine)
            };

            bool canLayOff = MeldRules.CanLayOff(new Card(CardSuit.Hearts, RankValues.Ten), run);

            Assert.IsFalse(canLayOff);
        }
    }
}
