using System.Collections.Generic;

using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Domain;
using NUnit.Framework;

namespace GinRummy.Server.Tests.Application.Gameplay
{
    [TestFixture]
    public class MatchRegistryTests
    {
        private const int DealerId = 1;
        private const int NonDealerId = 2;
        private const int DealerScore = 35;
        private const int NonDealerScore = 60;
        private const int DealerPublicTag = 1001;
        private const int NonDealerPublicTag = 1002;

        [Test]
        public void Test_Resume_Snapshot_DealsTheHandWithTheSavedDealer()
        {
            MatchRegistry matchRegistry = new MatchRegistry(StandardDeck.CreateOrdered);

            matchRegistry.Resume(CreateSnapshot());

            Assert.AreEqual(DealerId, matchRegistry.Find(NonDealerId).Round.DealerId);
        }

        [Test]
        public void Test_Resume_Snapshot_KeepsTheSavedScores()
        {
            MatchRegistry matchRegistry = new MatchRegistry(StandardDeck.CreateOrdered);

            matchRegistry.Resume(CreateSnapshot());

            Assert.AreEqual(DealerScore, matchRegistry.Find(DealerId).Match.GetScore(DealerId));
        }

        private static MatchSnapshot CreateSnapshot()
        {
            Dictionary<int, int> scoresByPlayerId = new Dictionary<int, int> { { DealerId, DealerScore }, { NonDealerId, NonDealerScore } };

            DuelPlayer dealer = new DuelPlayer(DealerId, "Dealer", DealerPublicTag);
            DuelPlayer nonDealer = new DuelPlayer(NonDealerId, "Non-dealer", NonDealerPublicTag);

            return new MatchSnapshot(dealer, nonDealer, scoresByPlayerId);
        }
    }
}
