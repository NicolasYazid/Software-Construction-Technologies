using System;
using System.Data;
using System.ServiceModel;
using System.Threading;

using GinRummy.Application.Ports;
using GinRummy.Contracts.Callbacks;
using GinRummy.Contracts.Faults;
using GinRummy.Contracts.Services;
using GinRummy.Domain;
using GinRummy.Domain.Entities.PlayerPresence;
using GinRummy.Services.Notifiers;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class PlayerPresenceService : IPlayerPresenceService
    {
        private const int NoPlayerId = 0;

        private readonly IPlayerPresenceOrchestrator _orchestrator;
        private readonly PlayerPresenceAnnouncer _announcer;
        private readonly ILogger<PlayerPresenceService> _logger;
        private int _playerId;

        public PlayerPresenceService(
            IPlayerPresenceOrchestrator orchestrator,
            PlayerPresenceAnnouncer announcer,
            ILogger<PlayerPresenceService> logger)
        {
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            _announcer = announcer ?? throw new ArgumentNullException(nameof(announcer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playerId = NoPlayerId;
        }

        public void SubscribeToPlayerPresence(string accessToken)
        {
            OperationContext context = OperationContext.Current;
            IPlayerPresenceCallback callback = context.GetCallbackChannel<IPlayerPresenceCallback>();
            try
            {
                int playerId = _orchestrator.IdentifyByAccessToken(accessToken);
                _announcer.RegisterPlayerConnection(playerId, callback);
                Interlocked.Exchange(ref _playerId, playerId);
                context.Channel.Closed += OnChannelClosed;
                context.Channel.Faulted += OnChannelFaulted;
                _orchestrator.AnnouncePlayerStatus(playerId, PlayerPresenceStatus.Online, _announcer.GetOnlinePlayers());
                _orchestrator.ReportCurrentPresenceStatusOfPlayers(playerId, _announcer.GetOnlinePlayers());
            }
            catch (InvalidAccessTokenException ex)
            {
                _logger.LogWarning(ex, "A client tried to follow the presence of the players with an access token that is not valid.");
                throw CreateServiceFault(ServiceFaultCode.SessionExpired);
            }
        }

        public void UnsubscribeFromPlayerPresence()
        {
            Disconnect();
        }

        private static FaultException<ServiceFault> CreateServiceFault(ServiceFaultCode code)
        {
            return new FaultException<ServiceFault>(new ServiceFault { Code = code });
        }

        private void Disconnect()
        {
            int playerId = Interlocked.Exchange(ref _playerId, NoPlayerId);
            if (playerId == NoPlayerId)
            {
                return;
            }

            _announcer.UnregisterPlayerConnection(playerId);
            try
            {
                _orchestrator.AnnouncePlayerStatus(playerId, PlayerPresenceStatus.Offline, _announcer.GetOnlinePlayers());
            }
            catch (DataException ex)
            {
                _logger.LogWarning(
                    ex,
                    "The other players were not told that player {PlayerId} left, because the database failed.",
                    playerId);
            }
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            Disconnect();
        }
    }
}
