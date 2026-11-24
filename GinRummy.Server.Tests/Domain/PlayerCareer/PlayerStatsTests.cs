using GinRummy.Domain.Entities.PlayerCareer;

using NUnit.Framework;

namespace GinRummy.Server.Tests.Domain.PlayerCareer
{
    [TestFixture]
    public class PlayerStatsTests
    {
        private const int PlayerAccountId = 1;
        private const int InitialScore = 1000;

        [Test]
        public void Test_RecordWin_NewPlayer_AddsThePointsOfAMatch()
        {
            PlayerStats stats = new PlayerStats(PlayerAccountId);

            stats.RecordWin();

            Assert.AreEqual(InitialScore + PlayerStats.PointsPerMatch, stats.Score);
        }

        [Test]
        public void Test_RecordLoss_NewPlayer_SubtractsThePointsOfAMatch()
        {
            PlayerStats stats = new PlayerStats(PlayerAccountId);

            stats.RecordLoss();

            Assert.AreEqual(InitialScore - PlayerStats.PointsPerMatch, stats.Score);
        }

        [Test]
        public void Test_RecordLoss_ScoreAboutToRunOut_LeavesTheScoreAtZero()
        {
            PlayerStats stats = new PlayerStats(PlayerAccountId);
            for (int lossCount = 0; lossCount < InitialScore / PlayerStats.PointsPerMatch; lossCount++)
            {
                stats.RecordLoss();
            }

            stats.RecordLoss();

            Assert.AreEqual(0, stats.Score);
        }
    }
}
