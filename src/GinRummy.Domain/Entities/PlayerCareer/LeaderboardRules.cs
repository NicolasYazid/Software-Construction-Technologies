using System;
using System.Collections.Generic;
using System.Linq;

namespace GinRummy.Domain.Entities.PlayerCareer
{
    public static class LeaderboardRules
    {
        public static IList<PlayerStats> OrderByLeaderboardRules(IList<PlayerStats> stats, IDictionary<int, string> usernames)
        {
            if (stats == null)
            {
                throw new ArgumentNullException(nameof(stats));
            }

            if (usernames == null)
            {
                throw new ArgumentNullException(nameof(usernames));
            }

            IList<PlayerStats> orderedStats = stats
                .OrderByDescending(playerStats => playerStats.Wins)
                .ThenByDescending(playerStats => playerStats.Score)
                .ThenBy(playerStats => playerStats.Losses)
                .ThenBy(playerStats => usernames[playerStats.PlayerAccountId])
                .ToList();

            return orderedStats;
        }
    }
}
