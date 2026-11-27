using System.Collections.Generic;

using GinRummy.Domain;
using NUnit.Framework;

namespace GinRummy.Server.Tests.Domain.Gameplay
{
    [TestFixture]
    public class MeldFinderTests
    {
        private const int LooseKingDeadwood = 10;
        private const int TwoFiveAndKingDeadwood = 17;

        [Test]
        public void Test_CountDeadwood_TwoRunsAndSetWithLooseKing_ReturnsTen()
        {
            IList<Card> hand = new List<Card>
            {
                new Card(CardSuit.Hearts, Card.AceRank),
                new Card(CardSuit.Hearts, RankValues.Two),
                new Card(CardSuit.Hearts, RankValues.Three),
                new Card(CardSuit.Spades, RankValues.Four),
                new Card(CardSuit.Spades, RankValues.Five),
                new Card(CardSuit.Spades, RankValues.Six),
                new Card(CardSuit.Clubs, RankValues.Nine),
                new Card(CardSuit.Diamonds, RankValues.Nine),
                new Card(CardSuit.Spades, RankValues.Nine),
                new Card(CardSuit.Diamonds, Card.KingRank)
            };

            int deadwood = MeldFinder.CountDeadwood(hand);

            Assert.AreEqual(LooseKingDeadwood, deadwood);
        }

        [Test]
        public void Test_CountDeadwood_CardFitsRunAndSet_ReturnsZero()
        {
            IList<Card> hand = new List<Card>
            {
                new Card(CardSuit.Hearts, RankValues.Four),
                new Card(CardSuit.Hearts, RankValues.Five),
                new Card(CardSuit.Hearts, RankValues.Six),
                new Card(CardSuit.Hearts, RankValues.Seven),
                new Card(CardSuit.Spades, RankValues.Seven),
                new Card(CardSuit.Clubs, RankValues.Seven)
            };

            int deadwood = MeldFinder.CountDeadwood(hand);

            Assert.AreEqual(0, deadwood);
        }

        [Test]
        public void Test_CountDeadwood_NoMelds_ReturnsSumOfCards()
        {
            IList<Card> hand = new List<Card>
            {
                new Card(CardSuit.Hearts, RankValues.Two),
                new Card(CardSuit.Spades, RankValues.Five),
                new Card(CardSuit.Clubs, Card.KingRank)
            };

            int deadwood = MeldFinder.CountDeadwood(hand);

            Assert.AreEqual(TwoFiveAndKingDeadwood, deadwood);
        }

        [Test]
        public void Test_FindBestMelds_FourCardsOfSameRank_ReturnsOneMeld()
        {
            IList<Card> nines = new List<Card>
            {
                new Card(CardSuit.Hearts, RankValues.Nine),
                new Card(CardSuit.Spades, RankValues.Nine),
                new Card(CardSuit.Clubs, RankValues.Nine),
                new Card(CardSuit.Diamonds, RankValues.Nine)
            };

            IList<IList<Card>> melds = MeldFinder.FindBestMelds(nines);

            Assert.AreEqual(1, melds.Count);
        }
    }
}
