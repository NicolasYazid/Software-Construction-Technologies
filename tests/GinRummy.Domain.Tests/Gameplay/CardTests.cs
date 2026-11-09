using System;

using GinRummy.Domain.Gameplay;
using NUnit.Framework;

namespace GinRummy.Domain.Tests.Gameplay
{
    [TestFixture]
    public class CardTests
    {
        [Test]
        public void Test_DeadwoodValue_FaceCard_ReturnsTen()
        {
            Card queen = new Card(CardSuit.Hearts, 12);

            int deadwoodValue = queen.DeadwoodValue;

            Assert.AreEqual(10, deadwoodValue);
        }

        [Test]
        public void Test_DeadwoodValue_Ace_ReturnsOne()
        {
            Card ace = new Card(CardSuit.Spades, Card.AceRank);

            int deadwoodValue = ace.DeadwoodValue;

            Assert.AreEqual(1, deadwoodValue);
        }

        [Test]
        public void Test_Constructor_RankAboveKing_ThrowsArgumentOutOfRangeException()
        {
            int rankAboveKing = Card.KingRank + 1;

            TestDelegate createCard = () => new Card(CardSuit.Clubs, rankAboveKing);

            Assert.Throws<ArgumentOutOfRangeException>(createCard);
        }

        [Test]
        public void Test_Equals_SameSuitAndRank_ReturnsTrue()
        {
            Card sevenOfHearts = new Card(CardSuit.Hearts, 7);

            bool isEqual = sevenOfHearts.Equals(new Card(CardSuit.Hearts, 7));

            Assert.IsTrue(isEqual);
        }
    }
}
