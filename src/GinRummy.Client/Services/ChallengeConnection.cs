using System;
using System.ServiceModel;
using System.Threading;

using GinRummy.Client.ChallengeServiceReference;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.Services
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public sealed class ChallengeConnection : IChallengeServiceCallback
    {
        private readonly Func<InstanceContext, IChallengeService> _serviceFactory;
        private readonly ILogger<ChallengeConnection> _logger;
        private readonly SynchronizationContext _screenContext;
        private IChallengeService _service;

        public ChallengeConnection(Func<InstanceContext, IChallengeService> serviceFactory, ILogger<ChallengeConnection> logger)
        {
            _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _screenContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public event EventHandler<Tuple<string, int>> ChallengeReceived;
        public event EventHandler<int> ChallengeCancelled;
        public event EventHandler<int> ChallengeDeclined;
        public event EventHandler RoundReady;

        public bool Start(string accessToken)
        {
            IChallengeService service = _serviceFactory(new InstanceContext(this));
            ICommunicationObject channel = (ICommunicationObject)service;
            bool isSubscribed = TryCall("subscribe to the challenges", () => service.SubscribeToChallenges(accessToken));
            if (isSubscribed)
            {
                channel.Faulted += OnChannelFaulted;
                _service = service;
            }
            else
            {
                channel.Abort();
            }

            return isSubscribed;
        }

        public bool Challenge(string accessToken, int challengedPublicTag)
        {
            return Call("challenge the player", service => service.ChallengePlayer(accessToken, challengedPublicTag));
        }

        public bool Cancel(string accessToken)
        {
            return Call("cancel the challenge", service => service.CancelChallenge(accessToken));
        }

        public bool Accept(string accessToken, int challengerPublicTag)
        {
            return Call("accept the challenge", service => service.AcceptChallenge(accessToken, challengerPublicTag));
        }

        public bool Decline(string accessToken, int challengerPublicTag)
        {
            return Call("decline the challenge", service => service.DeclineChallenge(accessToken, challengerPublicTag));
        }

        public void Stop()
        {
            IChallengeService service = _service;
            if (service == null)
            {
                return;
            }

            _service = null;
            ICommunicationObject channel = (ICommunicationObject)service;
            bool isClosed = TryCall("unsubscribe from the challenges", () =>
            {
                service.UnsubscribeFromChallenges();
                channel.Close();
            });
            if (!isClosed)
            {
                channel.Abort();
            }
        }

        public void OnChallengeReceived(string challengerName, int challengerPublicTag)
        {
            Tuple<string, int> challenger = Tuple.Create(challengerName, challengerPublicTag);
            _screenContext.Post(state => ChallengeReceived?.Invoke(this, challenger), null);
        }

        public void OnChallengeCancelled(int challengerPublicTag)
        {
            _screenContext.Post(state => ChallengeCancelled?.Invoke(this, challengerPublicTag), null);
        }

        public void OnChallengeDeclined(int challengedPublicTag)
        {
            _screenContext.Post(state => ChallengeDeclined?.Invoke(this, challengedPublicTag), null);
        }

        public void OnRoundReady()
        {
            _screenContext.Post(state => RoundReady?.Invoke(this, EventArgs.Empty), null);
        }

        private bool Call(string operation, Action<IChallengeService> call)
        {
            IChallengeService service = _service;

            return (service != null) && TryCall(operation, () => call(service));
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            _logger.LogWarning("The channel of the challenges failed, so no challenge arrives anymore.");
        }
        private bool TryCall(string operation, Action call)
        {
            bool isDone = false;
            try
            {
                call();
                isDone = true;
            }
            catch (FaultException<ServiceFault> ex)
            {
                _logger.LogWarning(ex, "The server refused to {Operation} with the fault {FaultCode}.", operation, ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, "The server could not {Operation}.", operation);
            }
            catch (CommunicationException ex)
            {
                _logger.LogError(ex, "The service could not be reached to {Operation}.", operation);
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "The service timed out while trying to {Operation}.", operation);
            }

            return isDone;
        }    }
}