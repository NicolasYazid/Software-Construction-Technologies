using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace GinRummy.Domain.Tests.Gameplay
{
    [TestFixture]
    public class StandardDeckTests
    {
        private const int Seed = 7;

        [Test]
        public void Test_CreateShuffled_AnySeed_HasTheFiftyTwoDifferentCards()
        {
            IList<Card> deck = StandardDeck.CreateShuffled(new Random(Seed));

            int differentCards = deck.Distinct().Count();

            Assert.AreEqual(StandardDeck.CardCount, differentCards);
        }

        [Test]
        public void Test_CreateShuffled_SameSeed_ReturnsTheSameOrder()
        {
            IList<Card> firstDeck = StandardDeck.CreateShuffled(new Random(Seed));

            IList<Card> secondDeck = StandardDeck.CreateShuffled(new Random(Seed));

            CollectionAssert.AreEqual(firstDeck, secondDeck);
        }
    }
}
