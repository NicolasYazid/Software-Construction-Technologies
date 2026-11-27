using System;
using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerCareer;
using NUnit.Framework;

namespace GinRummy.Server.Tests.Domain.PlayerCareer
{
    [TestFixture]
    public class LeaderboardRulesTests
    {
        private const int FirstPlayerId = 1;
        private const int SecondPlayerId = 2;
        private const string FirstUsername = "Ana";
        private const string SecondUsername = "Beto";
        private const int LossesToEmptyNewScore = 40;
        private const int LossesToEmptyScoreAfterOneWin = 41;
        private const int LossesToEmptyScoreAfterTwoWins = 42;
        private const int SecondPlayerWins = 2;

        private IDictionary<int, string> _usernames;

        [SetUp]
        public void SetUp()
        {
            _usernames = new Dictionary<int, string>
            {
                { FirstPlayerId, FirstUsername },
                { SecondPlayerId, SecondUsername }
            };
        }

        [Test]
        public void Test_OrderByLeaderboardRules_SecondPlayerHasMoreWins_PutsSecondPlayerFirst()
        {
            PlayerStats firstStats = new PlayerStats(FirstPlayerId);
            PlayerStats secondStats = new PlayerStats(SecondPlayerId);
            RecordWins(firstStats, 1);
            RecordLosses(firstStats, LossesToEmptyScoreAfterOneWin);
            RecordWins(secondStats, SecondPlayerWins);
            RecordLosses(secondStats, LossesToEmptyScoreAfterTwoWins);
            List<PlayerStats> stats = new List<PlayerStats> { firstStats, secondStats };

            IList<PlayerStats> orderedStats = LeaderboardRules.OrderByLeaderboardRules(stats, _usernames);

            Assert.AreEqual(SecondPlayerId, orderedStats[0].PlayerAccountId);
        }

        [Test]
        public void Test_OrderByLeaderboardRules_SameWinsAndSecondPlayerHasMoreScore_PutsSecondPlayerFirst()
        {
            PlayerStats firstStats = new PlayerStats(FirstPlayerId);
            PlayerStats secondStats = new PlayerStats(SecondPlayerId);
            RecordWins(firstStats, 1);
            RecordLosses(firstStats, LossesToEmptyScoreAfterOneWin);
            RecordLosses(secondStats, LossesToEmptyScoreAfterOneWin);
            RecordWins(secondStats, 1);
            List<PlayerStats> stats = new List<PlayerStats> { firstStats, secondStats };

            IList<PlayerStats> orderedStats = LeaderboardRules.OrderByLeaderboardRules(stats, _usernames);

            Assert.AreEqual(SecondPlayerId, orderedStats[0].PlayerAccountId);
        }

        [Test]
        public void Test_OrderByLeaderboardRules_SameWinsAndScoreAndSecondPlayerHasFewerLosses_PutsSecondPlayerFirst()
        {
            PlayerStats firstStats = new PlayerStats(FirstPlayerId);
            PlayerStats secondStats = new PlayerStats(SecondPlayerId);
            RecordLosses(firstStats, LossesToEmptyScoreAfterOneWin);
            RecordLosses(secondStats, LossesToEmptyNewScore);
            List<PlayerStats> stats = new List<PlayerStats> { firstStats, secondStats };

            IList<PlayerStats> orderedStats = LeaderboardRules.OrderByLeaderboardRules(stats, _usernames);

            Assert.AreEqual(SecondPlayerId, orderedStats[0].PlayerAccountId);
        }

        [Test]
        public void Test_OrderByLeaderboardRules_SameStatsAndFirstUsernameComesFirst_PutsFirstPlayerFirst()
        {
            PlayerStats firstStats = new PlayerStats(FirstPlayerId);
            PlayerStats secondStats = new PlayerStats(SecondPlayerId);
            List<PlayerStats> stats = new List<PlayerStats> { secondStats, firstStats };

            IList<PlayerStats> orderedStats = LeaderboardRules.OrderByLeaderboardRules(stats, _usernames);

            Assert.AreEqual(FirstPlayerId, orderedStats[0].PlayerAccountId);
        }

        [Test]
        public void Test_OrderByLeaderboardRules_NullStats_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => LeaderboardRules.OrderByLeaderboardRules(null, _usernames));
        }

        [Test]
        public void Test_OrderByLeaderboardRules_NullUsernames_ThrowsArgumentNullException()
        {
            List<PlayerStats> stats = new List<PlayerStats>
            {
                new PlayerStats(FirstPlayerId)
            };

            Assert.Throws<ArgumentNullException>(() => LeaderboardRules.OrderByLeaderboardRules(stats, null));
        }

        private static void RecordWins(PlayerStats stats, int winCount)
        {
            for (int recordedWins = 0; recordedWins < winCount; recordedWins++)
            {
                stats.RecordWin();
            }
        }

        private static void RecordLosses(PlayerStats stats, int lossCount)
        {
            for (int recordedLosses = 0; recordedLosses < lossCount; recordedLosses++)
            {
                stats.RecordLoss();
            }
        }
    }
}
