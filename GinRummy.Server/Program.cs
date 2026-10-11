using System;
using System.ServiceModel;

using GinRummy.Application.UseCases;
using GinRummy.Contracts;
using GinRummy.Data.EntityFramework.Repositories;
using GinRummy.Domain.Repositories;
using GinRummy.Domain.Services;
using GinRummy.Server.Services;

namespace GinRummy.Server
{
    public class Program
    {
        private const string ConnectionStringName = "GinRummyDb";
        private const string ServiceAddress = "net.tcp://localhost:8000/RankingsService";

        public static void Main(string[] args)
        {
            RankingsService rankingsService = ComposeRankingsService();
            using (ServiceHost host = new ServiceHost(rankingsService))
            {
                host.AddServiceEndpoint(typeof(IRankingService), new NetTcpBinding(), ServiceAddress);
                host.Open();
                // The host serves requests on its own threads.
                // Blocking here keeps the process alive and listening until someone presses Enter.
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
