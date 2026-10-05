using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Contracts;

namespace GinRummy.Client.Network
{
    public class RankingsServiceProxy : IRankingService
    {
        private readonly ChannelFactory<IRankingService> _channelFactory;

        /// <summary>
        /// Builds the adapter against the server endpoint the composition root provides.
        /// </summary>
        /// <param name="serverAddress">The net.tcp address where the server exposes the service.</param>
        public RankingsServiceProxy(string serverAddress)
        {
            NetTcpBinding binding = new NetTcpBinding();
            EndpointAddress endpointAddress = new EndpointAddress(serverAddress);
            _channelFactory = new ChannelFactory<IRankingService>(binding, endpointAddress);
        }

        /// <summary>
        /// Gets the current leaderboard by asking the server.
        /// </summary>
        /// <returns>The ranking rows the server returns.</returns>
        public List<PlayerRankingDto> GetRankings()
        {
            IRankingService channel = _channelFactory.CreateChannel();
            List<PlayerRankingDto> rankings = channel.GetRankings();
            ((IClientChannel)channel).Close();

            return rankings;
        }
    }
}
