using System.Linq;

using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Domain;
using NUnit.Framework;

namespace GinRummy.Application.Tests.Gameplay
{
    [TestFixture]
    public class PlayRoundUseCaseTests
    {
        private const int DealerId = 1;
        private const int NonDealerId = 2;
        private const int PlayerWithoutRoundId = 3;

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
            PlayRoundUseCase useCase = new PlayRoundUseCase(matchRegistry, announcer);

            PlayUntilTheStockRunsOut(useCase, matchRegistry);

            CollectionAssert.AreEquivalent(new[] { DealerId, NonDealerId }, announcer.FinishedPlayerIds);
        }

        [Test]
        public void Test_Discard_StockRunsOut_DealsTheNextRoundWithTheDealerSwapped()
        {
            MatchRegistry matchRegistry = BuildRegistryWithMatch();
            PlayRoundUseCase useCase = new PlayRoundUseCase(matchRegistry, new FakeRoundAnnouncer());

            PlayUntilTheStockRunsOut(useCase, matchRegistry);

            Assert.AreEqual(NonDealerId, matchRegistry.Find(DealerId).Round.DealerId);
        }

        private static PlayRoundUseCase CreateUseCaseWithRound(FakeRoundAnnouncer announcer)
        {
            return new PlayRoundUseCase(BuildRegistryWithMatch(), announcer);
        }

        private static MatchRegistry BuildRegistryWithMatch()
        {
            MatchRegistry matchRegistry = new MatchRegistry();
            matchRegistry.Start(new DuelPlayer(DealerId, "Dealer", 1001), new DuelPlayer(NonDealerId, "Non-dealer", 1002));

            return matchRegistry;
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
