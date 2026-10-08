using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Contracts;

namespace GinRummy.Client.Network
{
    public class RankingsServiceProxy : IRankingService
    {
        private readonly ChannelFactory<IRankingService> _channelFactory;

        public RankingsServiceProxy(string serverAddress)
        {
            NetTcpBinding binding = new NetTcpBinding();
            EndpointAddress endpointAddress = new EndpointAddress(serverAddress);
            _channelFactory = new ChannelFactory<IRankingService>(binding, endpointAddress);
        }

        public List<PlayerRankingDto> GetRankings()
        {
            IRankingService channel = _channelFactory.CreateChannel();
            List<PlayerRankingDto> rankings = channel.GetRankings();
            ((IClientChannel)channel).Close();

            return rankings;
        }
    }
}
