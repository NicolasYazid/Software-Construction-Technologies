using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Domain;
using NUnit.Framework;

namespace GinRummy.Server.Tests.Domain.Gameplay
{
    [TestFixture]
    public class GameRoundTests
    {
        private const int NonDealerId = 1;
        private const int DealerId = 2;
        private const int HandsDealt = 2;
        private const int DefenderDeadwood = 66;
        private const int KnockerDeadwood = 2;
        private const int LaidOffTenDeadwood = 10;

        [Test]
        public void Test_Constructor_FullDeck_DealsTheCardsOfAHandToEachPlayer()
        {
            GameRound round = new GameRound(DealerId, NonDealerId, StandardDeck.CreateOrdered());

            int dealerCardCount = round.GetHand(DealerId).Count;

            Assert.AreEqual(RoundRules.CardsPerHand, dealerCardCount);
        }

        [Test]
        public void Test_Constructor_FullDeck_LeavesTheRestInTheStock()
        {
            int expectedStockCount = StandardDeck.CardCount - (HandsDealt * RoundRules.CardsPerHand) - 1;

            GameRound round = new GameRound(DealerId, NonDealerId, StandardDeck.CreateOrdered());

            Assert.AreEqual(expectedStockCount, round.StockCount);
        }

        [Test]
        public void Test_Constructor_IncompleteDeck_ThrowsArgumentException()
        {
            IList<Card> incompleteDeck = StandardDeck.CreateOrdered().Skip(1).ToList();

            TestDelegate createRound = () => new GameRound(DealerId, NonDealerId, incompleteDeck);

            Assert.Throws<ArgumentException>(createRound);
        }

        [Test]
        public void Test_TakeFirstCard_NonDealer_AddsTheFaceUpCardToTheHand()
        {
            Card faceUpCard = new Card(CardSuit.Diamonds, RankValues.Two);
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), faceUpCard);

            round.TakeFirstCard(NonDealerId);

            CollectionAssert.Contains(round.GetHand(NonDealerId), faceUpCard);
        }

        [Test]
        public void Test_PassFirstCard_ByBothPlayers_GivesTheTurnToTheNonDealer()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, RankValues.Two));
            round.PassFirstCard(NonDealerId);

            round.PassFirstCard(DealerId);

            Assert.AreEqual(NonDealerId, round.CurrentPlayerId);
        }

        [Test]
        public void Test_DrawFromDiscardPile_AfterBothPlayersPass_ThrowsInvalidMoveException()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, RankValues.Two));
            round.PassFirstCard(NonDealerId);
            round.PassFirstCard(DealerId);

            TestDelegate drawFromDiscardPile = () => round.DrawFromDiscardPile(NonDealerId);

            Assert.Throws<InvalidMoveException>(drawFromDiscardPile);
        }

        [Test]
        public void Test_DrawFromStock_DealerAtTheStart_ThrowsInvalidMoveException()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, RankValues.Two));

            TestDelegate drawFromStock = () => round.DrawFromStock(DealerId);

            Assert.Throws<InvalidMoveException>(drawFromStock);
        }

        [Test]
        public void Test_Discard_CardJustTakenFromDiscardPile_ThrowsInvalidMoveException()
        {
            Card faceUpCard = new Card(CardSuit.Diamonds, RankValues.Two);
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), faceUpCard);
            round.TakeFirstCard(NonDealerId);

            TestDelegate discard = () => round.Discard(NonDealerId, faceUpCard);

            Assert.Throws<InvalidMoveException>(discard);
        }

        [Test]
        public void Test_Discard_CardOfTheOtherPlayer_ThrowsInvalidMoveException()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, RankValues.Two));
            round.TakeFirstCard(NonDealerId);

            TestDelegate discard = () => round.Discard(NonDealerId, new Card(CardSuit.Spades, RankValues.Jack));

            Assert.Throws<InvalidMoveException>(discard);
        }

        [Test]
        public void Test_Discard_BeforeDrawing_ThrowsInvalidMoveException()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, RankValues.Two));
            round.PassFirstCard(NonDealerId);
            round.PassFirstCard(DealerId);

            TestDelegate discard = () => round.Discard(NonDealerId, new Card(CardSuit.Diamonds, Card.KingRank));

            Assert.Throws<InvalidMoveException>(discard);
        }

        [Test]
        public void Test_Discard_AfterDrawing_GivesTheTurnToTheOpponent()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, RankValues.Two));
            round.PassFirstCard(NonDealerId);
            round.PassFirstCard(DealerId);
            round.DrawFromStock(NonDealerId);

            round.Discard(NonDealerId, new Card(CardSuit.Diamonds, Card.KingRank));

            Assert.AreEqual(DealerId, round.CurrentPlayerId);
        }

        [Test]
        public void Test_Discard_WhenTheStockReachesTheCardsToVoid_VoidsTheHand()
        {
            GameRound round = new GameRound(DealerId, NonDealerId, StandardDeck.CreateOrdered());
            round.PassFirstCard(NonDealerId);
            round.PassFirstCard(DealerId);
            int turnsBeforeTheLastDraw = round.StockCount - RoundRules.StockCardsToVoid - 1;
            for (int turn = 0; turn < turnsBeforeTheLastDraw; turn++)
            {
                DrawAndDiscardTheDrawnCard(round);
            }
            round.DrawFromStock(round.CurrentPlayerId);

            round.Discard(round.CurrentPlayerId, round.GetHand(round.CurrentPlayerId).Last());

            Assert.AreEqual(RoundOutcome.Void, round.Result.Outcome);
        }

        [Test]
        public void Test_Knock_DeadwoodAboveThreshold_ThrowsInvalidMoveException()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, RankValues.Jack));
            round.TakeFirstCard(NonDealerId);
            IList<IList<Card>> melds = new List<IList<Card>> { CreateSpadesRun(), CreateClubsRun() };

            TestDelegate knock = () => round.Knock(NonDealerId, new Card(CardSuit.Hearts, Card.AceRank), melds);

            Assert.Throws<InvalidMoveException>(knock);
        }

        [Test]
        public void Test_Knock_GroupThatIsNotAMeld_ThrowsInvalidMoveException()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, RankValues.Two));
            round.TakeFirstCard(NonDealerId);
            IList<Card> notAMeld = new List<Card>
            {
                new Card(CardSuit.Hearts, Card.AceRank),
                new Card(CardSuit.Hearts, RankValues.Two),
                new Card(CardSuit.Spades, RankValues.Four)
            };
            IList<IList<Card>> melds = new List<IList<Card>> { notAMeld };

            TestDelegate knock = () => round.Knock(NonDealerId, new Card(CardSuit.Diamonds, Card.KingRank), melds);

            Assert.Throws<InvalidMoveException>(knock);
        }

        [Test]
        public void Test_LayDownDefenderHand_KnockerWithLessDeadwood_ScoresTheDifference()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, RankValues.Two));
            round.TakeFirstCard(NonDealerId);
            round.Knock(NonDealerId, new Card(CardSuit.Diamonds, Card.KingRank), CreateKnockerMelds());

            RoundResult result = round.LayDownDefenderHand(DealerId, new List<IList<Card>>());

            Assert.AreEqual(DefenderDeadwood - KnockerDeadwood, result.Points);
        }

        [Test]
        public void Test_LayDownDefenderHand_KnockWithoutUnmatchedCards_AddsTheGinBonus()
        {
            Card tenOfClubs = new Card(CardSuit.Clubs, RankValues.Ten);
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), tenOfClubs);
            round.TakeFirstCard(NonDealerId);
            IList<Card> longClubsRun = CreateClubsRun();
            longClubsRun.Add(tenOfClubs);
            IList<IList<Card>> melds = new List<IList<Card>> { CreateHeartsRun(), CreateSpadesRun(), longClubsRun };
            round.Knock(NonDealerId, new Card(CardSuit.Diamonds, Card.KingRank), melds);

            RoundResult result = round.LayDownDefenderHand(DealerId, new List<IList<Card>>());

            Assert.AreEqual(DefenderDeadwood + RoundRules.GinBonus, result.Points);
        }

        [Test]
        public void Test_LayDownDefenderHand_DefenderWithLessDeadwood_IsUndercut()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHandOfSets(), new Card(CardSuit.Diamonds, RankValues.Nine));
            round.TakeFirstCard(NonDealerId);
            round.Knock(NonDealerId, new Card(CardSuit.Diamonds, Card.KingRank), CreateKnockerMelds());

            RoundResult result = round.LayDownDefenderHand(DealerId, CreateDefenderSets());

            Assert.AreEqual(RoundOutcome.Undercut, result.Outcome);
        }

        [Test]
        public void Test_LayDownDefenderHand_CardThatFitsAKnockerMeld_IsLaidOff()
        {
            IList<Card> defenderHand = CreateDefenderHand();
            defenderHand[defenderHand.Count - 1] = new Card(CardSuit.Clubs, RankValues.Ten);
            GameRound round = StartRound(CreateKnockerHand(), defenderHand, new Card(CardSuit.Diamonds, RankValues.Two));
            round.TakeFirstCard(NonDealerId);
            round.Knock(NonDealerId, new Card(CardSuit.Diamonds, Card.KingRank), CreateKnockerMelds());

            RoundResult result = round.LayDownDefenderHand(DealerId, new List<IList<Card>>());

            Assert.AreEqual(DefenderDeadwood - LaidOffTenDeadwood - KnockerDeadwood, result.Points);
        }

        [Test]
        public void Test_LayDownDefenderHand_CardThatFitsAKnockerMeld_LeavesItOutOfTheDefenderDeadwood()
        {
            IList<Card> defenderHand = CreateDefenderHand();
            defenderHand[defenderHand.Count - 1] = new Card(CardSuit.Clubs, RankValues.Ten);
            GameRound round = StartRound(CreateKnockerHand(), defenderHand, new Card(CardSuit.Diamonds, RankValues.Two));
            round.TakeFirstCard(NonDealerId);
            round.Knock(NonDealerId, new Card(CardSuit.Diamonds, Card.KingRank), CreateKnockerMelds());

            round.LayDownDefenderHand(DealerId, new List<IList<Card>>());

            Assert.AreEqual(DefenderDeadwood - LaidOffTenDeadwood, round.DefenderDeadwood);
        }

        private static GameRound StartRound(IList<Card> nonDealerHand, IList<Card> dealerHand, Card faceUpCard)
        {
            List<Card> deck = new List<Card>();
            for (int index = 0; index < nonDealerHand.Count; index++)
            {
                deck.Add(nonDealerHand[index]);
                deck.Add(dealerHand[index]);
            }

            deck.Add(faceUpCard);
            List<Card> stock = StandardDeck.CreateOrdered().Where(card => !deck.Contains(card)).ToList();
            deck.AddRange(stock);

            return new GameRound(DealerId, NonDealerId, deck);
        }

        private static void DrawAndDiscardTheDrawnCard(GameRound round)
        {
            int playerId = round.CurrentPlayerId;
            round.DrawFromStock(playerId);
            round.Discard(playerId, round.GetHand(playerId).Last());
        }

        private static IList<Card> CreateKnockerHand()
        {
            List<Card> hand = new List<Card>();
            hand.AddRange(CreateHeartsRun());
            hand.AddRange(CreateSpadesRun());
            hand.AddRange(CreateClubsRun());
            hand.Add(new Card(CardSuit.Diamonds, Card.KingRank));

            return hand;
        }

        private static IList<IList<Card>> CreateKnockerMelds()
        {
            return new List<IList<Card>> { CreateHeartsRun(), CreateSpadesRun(), CreateClubsRun() };
        }

        private static IList<Card> CreateHeartsRun()
        {
            return new List<Card>
            {
                new Card(CardSuit.Hearts, Card.AceRank),
                new Card(CardSuit.Hearts, RankValues.Two),
                new Card(CardSuit.Hearts, RankValues.Three)
            };
        }

        private static IList<Card> CreateSpadesRun()
        {
            return new List<Card>
            {
                new Card(CardSuit.Spades, RankValues.Four),
                new Card(CardSuit.Spades, RankValues.Five),
                new Card(CardSuit.Spades, RankValues.Six)
            };
        }

        private static IList<Card> CreateClubsRun()
        {
            return new List<Card>
            {
                new Card(CardSuit.Clubs, RankValues.Seven),
                new Card(CardSuit.Clubs, RankValues.Eight),
                new Card(CardSuit.Clubs, RankValues.Nine)
            };
        }

        private static IList<Card> CreateDefenderHand()
        {
            return new List<Card>
            {
                new Card(CardSuit.Diamonds, Card.AceRank),
                new Card(CardSuit.Diamonds, RankValues.Three),
                new Card(CardSuit.Diamonds, RankValues.Five),
                new Card(CardSuit.Diamonds, RankValues.Seven),
                new Card(CardSuit.Diamonds, RankValues.Nine),
                new Card(CardSuit.Hearts, RankValues.Five),
                new Card(CardSuit.Hearts, RankValues.Seven),
                new Card(CardSuit.Hearts, RankValues.Nine),
                new Card(CardSuit.Spades, RankValues.Jack),
                new Card(CardSuit.Spades, Card.KingRank)
            };
        }

        private static IList<Card> CreateDefenderHandOfSets()
        {
            List<Card> hand = CreateDefenderSets().SelectMany(meld => meld).ToList();
            hand.Add(new Card(CardSuit.Diamonds, RankValues.Two));

            return hand;
        }

        private static IList<IList<Card>> CreateDefenderSets()
        {
            return new List<IList<Card>>
            {
                new List<Card>
                {
                    new Card(CardSuit.Hearts, Card.KingRank),
                    new Card(CardSuit.Spades, Card.KingRank),
                    new Card(CardSuit.Clubs, Card.KingRank)
                },
                new List<Card>
                {
                    new Card(CardSuit.Hearts, RankValues.Jack),
                    new Card(CardSuit.Diamonds, RankValues.Jack),
                    new Card(CardSuit.Clubs, RankValues.Jack)
                },
                new List<Card>
                {
                    new Card(CardSuit.Hearts, RankValues.Queen),
                    new Card(CardSuit.Diamonds, RankValues.Queen),
                    new Card(CardSuit.Clubs, RankValues.Queen)
                }
            };
        }
    }
}
