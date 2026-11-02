using System;
using System.ServiceModel;
using System.Threading;

using GinRummy.Client.PlayerPresenceServiceReference;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.Services
{
    // The callbacks arrive on threads of WCF, so each one is posted to the thread that created the monitor, which is the one of the screen.
    // The callbacks do not use the synchronization context of the screen, so a call made from the screen never waits for a callback.
    [CallbackBehavior(UseSynchronizationContext = false)]
    public sealed class PlayerPresenceMonitor : IPlayerPresenceServiceCallback
    {
        private readonly Func<InstanceContext, IPlayerPresenceService> _presenceServiceFactory;
        private readonly ILogger<PlayerPresenceMonitor> _logger;
        private readonly SynchronizationContext _screenContext;
        private IPlayerPresenceService _presenceService;

        public PlayerPresenceMonitor(
            Func<InstanceContext, IPlayerPresenceService> presenceServiceFactory,
            ILogger<PlayerPresenceMonitor> logger)
        {
            _presenceServiceFactory = presenceServiceFactory ?? throw new ArgumentNullException(nameof(presenceServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _screenContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public event EventHandler<PlayerPresenceReport> CurrentStatusReported;
        public event EventHandler<PlayerPresence> PlayerPresenceChanged;

        // A client is created for every start because a client whose channel failed cannot be opened again.
        public bool Start(string accessToken)
        {
            bool isSubscribed = false;
            IPlayerPresenceService presenceService = _presenceServiceFactory(new InstanceContext(this));
            ICommunicationObject communicationObject = (ICommunicationObject)presenceService;
            try
            {
                presenceService.SubscribeToPlayerPresence(accessToken);
                communicationObject.Faulted += OnChannelFaulted;
                _presenceService = presenceService;
                isSubscribed = true;
            }
            catch (FaultException<ServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogWarning(
                    ex,
                    "The server refused to follow the presence of the players with the fault {FaultCode}.",
                    ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not start the follow-up of the presence of the players.");
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The presence service could not be reached to follow the players.");
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The presence service timed out while starting to follow the players.");
            }

            return isSubscribed;
        }

        // The server marks the player as disconnected when the channel closes, so closing it is what lets the others know.
        public void Stop()
        {
            if (_presenceService == null)
            {
                return;
            }

            IPlayerPresenceService presenceService = _presenceService;
            _presenceService = null;
            ICommunicationObject communicationObject = (ICommunicationObject)presenceService;
            try
            {
                presenceService.UnsubscribeFromPlayerPresence();
                communicationObject.Close();
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogWarning(ex, "The channel of the presence of the players did not close cleanly.");
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogWarning(ex, "The channel of the presence of the players took too long to close.");
            }
        }

        public void OnCurrentStatusOfPlayersReported(PlayerPresenceReport report)
        {
            _screenContext.Post(state => CurrentStatusReported?.Invoke(this, report), null);
        }

        public void OnPlayerPresenceChanged(PlayerPresence presence)
        {
            _screenContext.Post(state => PlayerPresenceChanged?.Invoke(this, presence), null);
        }

        // The lists stay as they were, and the player has to open the lobby again to follow the presence of the others.
        private void OnChannelFaulted(object sender, EventArgs e)
        {
            _logger.LogWarning("The channel of the presence of the players failed, so the lists may no longer be up to date.");
        }
    }
}
