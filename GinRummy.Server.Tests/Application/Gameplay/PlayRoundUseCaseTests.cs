using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Domain;
using NUnit.Framework;

namespace GinRummy.Server.Tests.Application.Gameplay
{
    [TestFixture]
    public class PlayRoundUseCaseTests
    {
        private const int DealerId = 1;
        private const int NonDealerId = 2;
        private const int PlayerWithoutRoundId = 3;
        private const int DealerPublicTag = 1001;
        private const int NonDealerPublicTag = 1002;

        [Test]
        public void Test_PassFirstCard_NonDealer_AnnouncesTheChangeToBothPlayers()
        {
            FakeRoundAnnouncer announcer = new FakeRoundAnnouncer();
            PlayRoundUseCase useCase = CreateUseCaseWithRound(announcer);

            useCase.PassFirstCard(NonDealerId);

            CollectionAssert.AreEquivalent(new[] { DealerId, NonDealerId }, announcer.ChangedPlayerIds);
        }

        [Test]
        public void Test_DrawFromStock_OutOfTurn_ThrowsInvalidMoveException()
        {
            PlayRoundUseCase useCase = CreateUseCaseWithRound(new FakeRoundAnnouncer());

            TestDelegate drawFromStock = () => useCase.DrawFromStock(DealerId);

            Assert.Throws<InvalidMoveException>(drawFromStock);
        }

        [Test]
        public void Test_DrawFromStock_PlayerWithoutRound_ThrowsInvalidMoveException()
        {
            PlayRoundUseCase useCase = CreateUseCaseWithRound(new FakeRoundAnnouncer());

            TestDelegate drawFromStock = () => useCase.DrawFromStock(PlayerWithoutRoundId);

            Assert.Throws<InvalidMoveException>(drawFromStock);
        }

        [Test]
        public void Test_ForfeitRound_PlayerInRound_AnnouncesThatTheOpponentLeft()
        {
            FakeRoundAnnouncer announcer = new FakeRoundAnnouncer();
            PlayRoundUseCase useCase = CreateUseCaseWithRound(announcer);

            useCase.ForfeitRound(NonDealerId);

            CollectionAssert.AreEqual(new[] { DealerId }, announcer.LeftPlayerIds);
        }

        [Test]
        public void Test_ForfeitRound_PlayerInRound_RecordsTheOpponentAsWinner()
        {
            FakeMatchRecorder matchRecorder = new FakeMatchRecorder();
            PlayRoundUseCase useCase = new PlayRoundUseCase(BuildRegistryWithMatch(), new FakeRoundAnnouncer(), matchRecorder);

            useCase.ForfeitRound(NonDealerId);

            CollectionAssert.AreEqual(new[] { DealerId }, matchRecorder.WinnerIds);
        }

        [Test]
        public void Test_WaitForReconnection_PlayerInRound_TellsTheOpponent()
        {
            FakeRoundAnnouncer announcer = new FakeRoundAnnouncer();
            PlayRoundUseCase useCase = CreateUseCaseWithRound(announcer);

            useCase.WaitForReconnection(NonDealerId);

            CollectionAssert.AreEqual(new[] { DealerId }, announcer.DisconnectedNoticePlayerIds);
        }

        [Test]
        public void Test_PassFirstCard_OpponentDisconnected_ThrowsInvalidMoveException()
        {
            PlayRoundUseCase useCase = CreateUseCaseWithRound(new FakeRoundAnnouncer());
            useCase.WaitForReconnection(DealerId);

            TestDelegate passFirstCard = () => useCase.PassFirstCard(NonDealerId);

            Assert.Throws<InvalidMoveException>(passFirstCard);
        }

        [Test]
        public void Test_Rejoin_DisconnectedPlayer_TellsTheOpponent()
        {
            FakeRoundAnnouncer announcer = new FakeRoundAnnouncer();
            PlayRoundUseCase useCase = CreateUseCaseWithRound(announcer);
            useCase.WaitForReconnection(NonDealerId);

            useCase.Rejoin(NonDealerId);

            CollectionAssert.AreEqual(new[] { DealerId }, announcer.ReconnectedNoticePlayerIds);
        }

        [Test]
        public void Test_PassFirstCard_DisconnectedPlayerRejoined_AnnouncesTheChange()
        {
            FakeRoundAnnouncer announcer = new FakeRoundAnnouncer();
            PlayRoundUseCase useCase = CreateUseCaseWithRound(announcer);
            useCase.WaitForReconnection(DealerId);
            useCase.Rejoin(DealerId);

            useCase.PassFirstCard(NonDealerId);

            CollectionAssert.Contains(announcer.ChangedPlayerIds, NonDealerId);
        }

        [Test]
        public void Test_PassFirstCard_OpponentOfflineWhenTheMatchStarts_ThrowsInvalidMoveException()
        {
            FakeRoundAnnouncer announcer = new FakeRoundAnnouncer();
            announcer.OfflinePlayerIds.Add(DealerId);
            MatchRegistry matchRegistry = new MatchRegistry(StandardDeck.CreateOrdered);
            PlayRoundUseCase useCase = new PlayRoundUseCase(matchRegistry, announcer, new FakeMatchRecorder());
            matchRegistry.Start(
                new DuelPlayer(DealerId, "Dealer", DealerPublicTag),
                new DuelPlayer(NonDealerId, "Non-dealer", NonDealerPublicTag));

            TestDelegate passFirstCard = () => useCase.PassFirstCard(NonDealerId);

            Assert.Throws<InvalidMoveException>(passFirstCard);
        }

        [Test]
        public void Test_PassFirstCard_BothPlayersOnlineWhenTheMatchStarts_AnnouncesTheChange()
        {
            FakeRoundAnnouncer announcer = new FakeRoundAnnouncer();
            MatchRegistry matchRegistry = new MatchRegistry(StandardDeck.CreateOrdered);
            PlayRoundUseCase useCase = new PlayRoundUseCase(matchRegistry, announcer, new FakeMatchRecorder());
            matchRegistry.Start(
                new DuelPlayer(DealerId, "Dealer", DealerPublicTag),
                new DuelPlayer(NonDealerId, "Non-dealer", NonDealerPublicTag));

            useCase.PassFirstCard(NonDealerId);

            CollectionAssert.Contains(announcer.ChangedPlayerIds, NonDealerId);
        }

        [Test]
        public void Test_Rejoin_MatchAlreadyOver_ReturnsFalse()
        {
            PlayRoundUseCase useCase = CreateUseCaseWithRound(new FakeRoundAnnouncer());
            useCase.ForfeitRound(DealerId);

            bool hasMatchToShow = useCase.Rejoin(DealerId);

            Assert.IsFalse(hasMatchToShow);
        }

        [Test]
        public void Test_Rejoin_OpponentForfeitedWhileAway_AnnouncesTheVictory()
        {
            FakeRoundAnnouncer announcer = new FakeRoundAnnouncer();
            PlayRoundUseCase useCase = CreateUseCaseWithRound(announcer);
            useCase.WaitForReconnection(NonDealerId);
            useCase.ForfeitRound(DealerId);

            useCase.Rejoin(NonDealerId);

            CollectionAssert.AreEqual(new[] { NonDealerId, NonDealerId }, announcer.LeftPlayerIds);
        }

        [Test]
        public void Test_ForfeitRound_SecondTime_AnnouncesOnlyOnce()
        {
            FakeRoundAnnouncer announcer = new FakeRoundAnnouncer();
            PlayRoundUseCase useCase = CreateUseCaseWithRound(announcer);
            useCase.ForfeitRound(NonDealerId);

            useCase.ForfeitRound(NonDealerId);

            Assert.AreEqual(1, announcer.LeftPlayerIds.Count);
        }

        [Test]
        public void Test_Discard_StockRunsOut_AnnouncesTheFinishedRoundToBothPlayers()
        {
            FakeRoundAnnouncer announcer = new FakeRoundAnnouncer();
            MatchRegistry matchRegistry = BuildRegistryWithMatch();
            PlayRoundUseCase useCase = new PlayRoundUseCase(matchRegistry, announcer, new FakeMatchRecorder());

            PlayUntilTheStockRunsOut(useCase, matchRegistry);

            CollectionAssert.AreEquivalent(new[] { DealerId, NonDealerId }, announcer.FinishedPlayerIds);
        }

        [Test]
        public void Test_Discard_StockRunsOut_DealsTheNextRoundWithTheDealerSwapped()
        {
            MatchRegistry matchRegistry = BuildRegistryWithMatch();
            PlayRoundUseCase useCase = new PlayRoundUseCase(matchRegistry, new FakeRoundAnnouncer(), new FakeMatchRecorder());

            PlayUntilTheStockRunsOut(useCase, matchRegistry);

            Assert.AreEqual(NonDealerId, matchRegistry.Find(DealerId).Round.DealerId);
        }

        [Test]
        public void Test_Knock_WinnerReachesTheTarget_RemovesTheMatch()
        {
            Queue<IList<Card>> decks = new Queue<IList<Card>>();
            decks.Enqueue(BuildDeck(CreateKnockerHand(), CreateDefenderHand()));
            decks.Enqueue(BuildDeck(CreateDefenderHand(), CreateKnockerHand()));
            MatchRegistry matchRegistry = new MatchRegistry(decks.Dequeue);
            matchRegistry.Start(
                new DuelPlayer(DealerId, "Dealer", DealerPublicTag),
                new DuelPlayer(NonDealerId, "Non-dealer", NonDealerPublicTag));
            PlayRoundUseCase useCase = new PlayRoundUseCase(matchRegistry, new FakeRoundAnnouncer(), new FakeMatchRecorder());
            useCase.TakeFirstCard(NonDealerId);
            useCase.Knock(NonDealerId, new Card(CardSuit.Diamonds, Card.KingRank));
            useCase.PassFirstCard(DealerId);
            useCase.TakeFirstCard(NonDealerId);

            useCase.Knock(NonDealerId, new Card(CardSuit.Diamonds, Card.KingRank));

            Assert.IsNull(matchRegistry.Find(NonDealerId));
        }

        private static PlayRoundUseCase CreateUseCaseWithRound(FakeRoundAnnouncer announcer)
        {
            return new PlayRoundUseCase(BuildRegistryWithMatch(), announcer, new FakeMatchRecorder());
        }

        private static MatchRegistry BuildRegistryWithMatch()
        {
            MatchRegistry matchRegistry = new MatchRegistry(StandardDeck.CreateOrdered);
            matchRegistry.Start(
                new DuelPlayer(DealerId, "Dealer", DealerPublicTag),
                new DuelPlayer(NonDealerId, "Non-dealer", NonDealerPublicTag));

            return matchRegistry;
        }

        private static IList<Card> BuildDeck(IList<Card> nonDealerHand, IList<Card> dealerHand)
        {
            List<Card> deck = new List<Card>();
            for (int index = 0; index < nonDealerHand.Count; index++)
            {
                deck.Add(nonDealerHand[index]);
                deck.Add(dealerHand[index]);
            }

            deck.Add(new Card(CardSuit.Diamonds, RankValues.Two));
            deck.AddRange(StandardDeck.CreateOrdered().Where(card => !deck.Contains(card)));

            return deck;
        }

        private static IList<Card> CreateKnockerHand()
        {
            return new List<Card>
            {
                new Card(CardSuit.Hearts, Card.AceRank),
                new Card(CardSuit.Hearts, RankValues.Two),
                new Card(CardSuit.Hearts, RankValues.Three),
                new Card(CardSuit.Spades, RankValues.Four),
                new Card(CardSuit.Spades, RankValues.Five),
                new Card(CardSuit.Spades, RankValues.Six),
                new Card(CardSuit.Clubs, RankValues.Seven),
                new Card(CardSuit.Clubs, RankValues.Eight),
                new Card(CardSuit.Clubs, RankValues.Nine),
                new Card(CardSuit.Diamonds, Card.KingRank)
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

        private static void PlayUntilTheStockRunsOut(PlayRoundUseCase useCase, MatchRegistry matchRegistry)
        {
            useCase.PassFirstCard(NonDealerId);
            useCase.PassFirstCard(DealerId);
            GameRound firstRound = matchRegistry.Find(DealerId).Round;
            while (firstRound.Phase != TurnPhase.Finished)
            {
                int playerId = firstRound.CurrentPlayerId;
                useCase.DrawFromStock(playerId);
                useCase.Discard(playerId, firstRound.GetHand(playerId).Last());
            }
        }
    }
}
