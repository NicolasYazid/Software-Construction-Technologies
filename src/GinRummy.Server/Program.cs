using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

using GinRummy.Application.UseCases;
using GinRummy.Persistence.Repositories;
using GinRummy.Domain.Repositories;
using GinRummy.Domain.Services;
using GinRummy.Server.Hosting;
using GinRummy.Server.Logging;
using GinRummy.Server.Services;

using Microsoft.Extensions.Logging;

namespace GinRummy.Server
{
    /// <summary>
    /// Entry point and composition root of the server process. Each service gets its own host,
    /// built from the system.serviceModel section of App.config, so a failure in one service
    /// stays in that service.
    /// </summary>
    public class Program
    {
        private const string ConnectionStringName = "GinRummyDb";
        private const string LogFolderName = "Logs";
        private const string LogFileName = "GinRummy.Server.log";

        /// <summary>
        /// Composes and opens every service and keeps them listening until Enter is pressed.
        /// </summary>
        /// <param name="args">Command line arguments; not used.</param>
        public static void Main(string[] args)
        {
            using (ILoggerFactory loggerFactory = new FileLoggerFactory(BuildLogFilePath(), LogLevel.Information))
            {
                IErrorHandler faultShield = new FaultShieldErrorHandler(loggerFactory.CreateLogger<FaultShieldErrorHandler>());
                IServiceBehavior faultShieldBehavior = new FaultShieldBehavior(faultShield);
                ServiceHostGroup hostGroup = new ServiceHostGroup(loggerFactory.CreateLogger<ServiceHostGroup>());

                hostGroup.Add(CreateIsolatedHost(typeof(RankingsService), ComposeRankingsService, faultShieldBehavior));

                hostGroup.OpenAll();
                // The hosts serve requests on their own threads; block here so the process
                // stays alive and keeps listening until someone presses Enter.
                Console.ReadLine();
                hostGroup.CloseAll();
            }
        }

        // The address, binding, timeouts and throttling come from App.config, so the code holds no network values.
        private static ServiceHost CreateIsolatedHost(
            Type serviceType,
            Func<object> createInstance,
            IServiceBehavior faultShieldBehavior)
        {
            ServiceHost host = new ServiceHost(serviceType);
            host.Description.Behaviors.Add(new InstanceFactoryBehavior(createInstance));
            host.Description.Behaviors.Add(faultShieldBehavior);

            return host;
        }

        private static string BuildLogFilePath()
        {
            string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFolderName);

            return Path.Combine(logFolder, LogFileName);
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
