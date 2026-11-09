using System;
using System.Data;
using System.ServiceModel;
using System.Threading;

using GinRummy.Application.Ports.PlayerPresence;
using GinRummy.Contracts.Faults.Shared;
using GinRummy.Contracts.Services.PlayerPresence;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerPresence;
using GinRummy.Services.Notifiers;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Services.PlayerPresence
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class PlayerPresenceService : IPlayerPresenceService
    {
        private readonly IPlayerPresenceOrchestrator _orchestrator;
        private readonly PlayerPresenceAnnouncer _announcer;
        private readonly ILogger<PlayerPresenceService> _logger;
        private AccessTokenOwner _accessTokenOwner;

        public PlayerPresenceService(
            IPlayerPresenceOrchestrator orchestrator,
            PlayerPresenceAnnouncer announcer,
            ILogger<PlayerPresenceService> logger)
        {
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            _announcer = announcer ?? throw new ArgumentNullException(nameof(announcer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void SubscribeToPlayerPresence(string accessToken)
        {
            OperationContext context = OperationContext.Current;
            IPlayerPresenceCallback callback = context.GetCallbackChannel<IPlayerPresenceCallback>();
            try
            {
                AccessTokenOwner accessTokenOwner = _orchestrator.IdentifyByAccessToken(accessToken);
                _announcer.RegisterPlayerConnection(accessTokenOwner, callback);
                Interlocked.Exchange(ref _accessTokenOwner, accessTokenOwner);
                context.Channel.Closed += OnChannelClosed;
                context.Channel.Faulted += OnChannelFaulted;
                _orchestrator.AnnouncePlayerStatus(accessTokenOwner, PlayerPresenceStatus.Online, _announcer.GetOnlinePlayers());
                _orchestrator.ReportCurrentPresenceStatusOfPlayers(accessTokenOwner, _announcer.GetOnlinePlayers());
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
            AccessTokenOwner accessTokenOwner = Interlocked.Exchange(ref _accessTokenOwner, null);
            if (accessTokenOwner == null)
            {
                return;
            }

            _announcer.UnregisterPlayerConnection(accessTokenOwner);
            try
            {
                _orchestrator.AnnouncePlayerStatus(accessTokenOwner, PlayerPresenceStatus.Offline, _announcer.GetOnlinePlayers());
            }
            catch (DataException ex)
            {
                _logger.LogWarning(
                    ex,
                    "The other players were not told that {AccessTokenOwner} left, because the database failed.",
                    accessTokenOwner);
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
