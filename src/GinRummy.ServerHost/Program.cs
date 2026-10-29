using System;
using System.ServiceModel;

using GinRummy.Application.UseCases;
using GinRummy.Persistence.Repositories;
using GinRummy.Domain.Repositories;
using GinRummy.Domain.Services;
using GinRummy.Services.Services;

namespace GinRummy.ServerHost
{
    public class Program
    {
        private const string ConnectionStringName = "GinRummyDb";

        public static void Main()
        {
            RankingsService rankingsService = ComposeRankingsService();
            using (ServiceHost host = new ServiceHost(rankingsService))
            {
                host.Open();
                Console.ReadLine();
            }
        }

        private static RankingsService ComposeRankingsService()
        {
            IRankingRepository rankingRepository = new RankingRepository(ConnectionStringName);
            RankResolver rankResolver = new RankResolver();
            ViewLeaderboardUseCase useCase = new ViewLeaderboardUseCase(rankingRepository, rankResolver);
            RankingsService rankingsService = new RankingsService(useCase);

            return rankingsService;
        }
    }
}
