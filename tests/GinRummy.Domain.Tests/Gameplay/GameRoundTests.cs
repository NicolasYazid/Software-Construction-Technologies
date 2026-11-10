using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Domain.Gameplay;
using NUnit.Framework;

namespace GinRummy.Domain.Tests.Gameplay
{
    [TestFixture]
    public class GameRoundTests
    {
        private const int NonDealerId = 1;
        private const int DealerId = 2;

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
            int expectedStockCount = StandardDeck.CardCount - (2 * RoundRules.CardsPerHand) - 1;

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
            Card faceUpCard = new Card(CardSuit.Diamonds, 2);
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), faceUpCard);

            round.TakeFirstCard(NonDealerId);

            CollectionAssert.Contains(round.GetHand(NonDealerId), faceUpCard);
        }

        [Test]
        public void Test_PassFirstCard_ByBothPlayers_GivesTheTurnToTheNonDealer()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, 2));
            round.PassFirstCard(NonDealerId);

            round.PassFirstCard(DealerId);

            Assert.AreEqual(NonDealerId, round.CurrentPlayerId);
        }

        [Test]
        public void Test_DrawFromDiscardPile_AfterBothPlayersPass_ThrowsInvalidMoveException()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, 2));
            round.PassFirstCard(NonDealerId);
            round.PassFirstCard(DealerId);

            TestDelegate drawFromDiscardPile = () => round.DrawFromDiscardPile(NonDealerId);

            Assert.Throws<InvalidMoveException>(drawFromDiscardPile);
        }

        [Test]
        public void Test_DrawFromStock_DealerAtTheStart_ThrowsInvalidMoveException()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, 2));

            TestDelegate drawFromStock = () => round.DrawFromStock(DealerId);

            Assert.Throws<InvalidMoveException>(drawFromStock);
        }

        [Test]
        public void Test_Discard_CardJustTakenFromDiscardPile_ThrowsInvalidMoveException()
        {
            Card faceUpCard = new Card(CardSuit.Diamonds, 2);
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), faceUpCard);
            round.TakeFirstCard(NonDealerId);

            TestDelegate discard = () => round.Discard(NonDealerId, faceUpCard);

            Assert.Throws<InvalidMoveException>(discard);
        }

        [Test]
        public void Test_Discard_CardOfTheOtherPlayer_ThrowsInvalidMoveException()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, 2));
            round.TakeFirstCard(NonDealerId);

            TestDelegate discard = () => round.Discard(NonDealerId, new Card(CardSuit.Spades, 11));

            Assert.Throws<InvalidMoveException>(discard);
        }

        [Test]
        public void Test_Discard_BeforeDrawing_ThrowsInvalidMoveException()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, 2));
            round.PassFirstCard(NonDealerId);
            round.PassFirstCard(DealerId);

            TestDelegate discard = () => round.Discard(NonDealerId, new Card(CardSuit.Diamonds, 13));

            Assert.Throws<InvalidMoveException>(discard);
        }

        [Test]
        public void Test_Discard_AfterDrawing_GivesTheTurnToTheOpponent()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, 2));
            round.PassFirstCard(NonDealerId);
            round.PassFirstCard(DealerId);
            round.DrawFromStock(NonDealerId);

            round.Discard(NonDealerId, new Card(CardSuit.Diamonds, 13));

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
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, 11));
            round.TakeFirstCard(NonDealerId);
            IList<IList<Card>> melds = new List<IList<Card>> { CreateSpadesRun(), CreateClubsRun() };

            TestDelegate knock = () => round.Knock(NonDealerId, new Card(CardSuit.Hearts, Card.AceRank), melds);

            Assert.Throws<InvalidMoveException>(knock);
        }

        [Test]
        public void Test_Knock_GroupThatIsNotAMeld_ThrowsInvalidMoveException()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, 2));
            round.TakeFirstCard(NonDealerId);
            IList<Card> notAMeld = new List<Card>
            {
                new Card(CardSuit.Hearts, Card.AceRank), new Card(CardSuit.Hearts, 2), new Card(CardSuit.Spades, 4)
            };
            IList<IList<Card>> melds = new List<IList<Card>> { notAMeld };

            TestDelegate knock = () => round.Knock(NonDealerId, new Card(CardSuit.Diamonds, 13), melds);

            Assert.Throws<InvalidMoveException>(knock);
        }

        [Test]
        public void Test_LayDownDefenderHand_KnockerWithLessDeadwood_ScoresTheDifference()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), new Card(CardSuit.Diamonds, 2));
            round.TakeFirstCard(NonDealerId);
            round.Knock(NonDealerId, new Card(CardSuit.Diamonds, 13), CreateKnockerMelds());

            RoundResult result = round.LayDownDefenderHand(DealerId, new List<IList<Card>>());

            Assert.AreEqual(66 - 2, result.Points);
        }

        [Test]
        public void Test_LayDownDefenderHand_KnockWithoutUnmatchedCards_AddsTheGinBonus()
        {
            Card tenOfClubs = new Card(CardSuit.Clubs, 10);
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHand(), tenOfClubs);
            round.TakeFirstCard(NonDealerId);
            IList<Card> longClubsRun = CreateClubsRun();
            longClubsRun.Add(tenOfClubs);
            IList<IList<Card>> melds = new List<IList<Card>> { CreateHeartsRun(), CreateSpadesRun(), longClubsRun };
            round.Knock(NonDealerId, new Card(CardSuit.Diamonds, 13), melds);

            RoundResult result = round.LayDownDefenderHand(DealerId, new List<IList<Card>>());

            Assert.AreEqual(66 + RoundRules.GinBonus, result.Points);
        }

        [Test]
        public void Test_LayDownDefenderHand_DefenderWithLessDeadwood_IsUndercut()
        {
            GameRound round = StartRound(CreateKnockerHand(), CreateDefenderHandOfSets(), new Card(CardSuit.Diamonds, 9));
            round.TakeFirstCard(NonDealerId);
            round.Knock(NonDealerId, new Card(CardSuit.Diamonds, 13), CreateKnockerMelds());

            RoundResult result = round.LayDownDefenderHand(DealerId, CreateDefenderSets());

            Assert.AreEqual(RoundOutcome.Undercut, result.Outcome);
        }

        [Test]
        public void Test_LayDownDefenderHand_CardThatFitsAKnockerMeld_IsLaidOff()
        {
            IList<Card> defenderHand = CreateDefenderHand();
            defenderHand[defenderHand.Count - 1] = new Card(CardSuit.Clubs, 10);
            GameRound round = StartRound(CreateKnockerHand(), defenderHand, new Card(CardSuit.Diamonds, 2));
            round.TakeFirstCard(NonDealerId);
            round.Knock(NonDealerId, new Card(CardSuit.Diamonds, 13), CreateKnockerMelds());

            RoundResult result = round.LayDownDefenderHand(DealerId, new List<IList<Card>>());

            Assert.AreEqual(66 - 10 - 2, result.Points);
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
            hand.Add(new Card(CardSuit.Diamonds, 13));

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
                new Card(CardSuit.Hearts, Card.AceRank), new Card(CardSuit.Hearts, 2), new Card(CardSuit.Hearts, 3)
            };
        }

        private static IList<Card> CreateSpadesRun()
        {
            return new List<Card> { new Card(CardSuit.Spades, 4), new Card(CardSuit.Spades, 5), new Card(CardSuit.Spades, 6) };
        }

        private static IList<Card> CreateClubsRun()
        {
            return new List<Card> { new Card(CardSuit.Clubs, 7), new Card(CardSuit.Clubs, 8), new Card(CardSuit.Clubs, 9) };
        }

        private static IList<Card> CreateDefenderHand()
        {
            return new List<Card>
            {
                new Card(CardSuit.Diamonds, Card.AceRank), new Card(CardSuit.Diamonds, 3), new Card(CardSuit.Diamonds, 5),
                new Card(CardSuit.Diamonds, 7), new Card(CardSuit.Diamonds, 9), new Card(CardSuit.Hearts, 5),
                new Card(CardSuit.Hearts, 7), new Card(CardSuit.Hearts, 9), new Card(CardSuit.Spades, 11),
                new Card(CardSuit.Spades, 13)
            };
        }

        private static IList<Card> CreateDefenderHandOfSets()
        {
            List<Card> hand = CreateDefenderSets().SelectMany(meld => meld).ToList();
            hand.Add(new Card(CardSuit.Diamonds, 2));

            return hand;
        }

        private static IList<IList<Card>> CreateDefenderSets()
        {
            return new List<IList<Card>>
            {
                new List<Card> { new Card(CardSuit.Hearts, 13), new Card(CardSuit.Spades, 13), new Card(CardSuit.Clubs, 13) },
                new List<Card> { new Card(CardSuit.Hearts, 11), new Card(CardSuit.Diamonds, 11), new Card(CardSuit.Clubs, 11) },
                new List<Card> { new Card(CardSuit.Hearts, 12), new Card(CardSuit.Diamonds, 12), new Card(CardSuit.Clubs, 12) }
            };
        }
    }
}
