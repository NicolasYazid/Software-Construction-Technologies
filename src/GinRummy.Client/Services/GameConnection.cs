using System;
using System.ServiceModel;
using System.Threading;

using GinRummy.Client.GameServiceReference;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.Services
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public sealed class GameConnection : IGameServiceCallback
    {
        private readonly Func<InstanceContext, IGameService> _serviceFactory;
        private readonly ILogger<GameConnection> _logger;
        private readonly SynchronizationContext _screenContext;
        private IGameService _service;

        public GameConnection(Func<InstanceContext, IGameService> serviceFactory, ILogger<GameConnection> logger)
        {
            _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _screenContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public event EventHandler<RoundDetails> RoundStarted;
        public event EventHandler<RoundDetails> RoundChanged;
        public event EventHandler<RoundSummary> RoundFinished;
        public event EventHandler OpponentLeft;

        public bool Start(string accessToken)
        {
            IGameService service = _serviceFactory(new InstanceContext(this));
            ICommunicationObject channel = (ICommunicationObject)service;
            bool isSubscribed = TryCall("subscribe to the round", () => service.SubscribeToGame(accessToken));
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

        public bool TakeFirstCard(string accessToken)
        {
            return Call("take the first card", service => service.TakeFirstCard(accessToken));
        }

        public bool PassFirstCard(string accessToken)
        {
            return Call("pass the first card", service => service.PassFirstCard(accessToken));
        }

        public bool DrawFromStock(string accessToken)
        {
            return Call("draw from the stock", service => service.DrawFromStock(accessToken));
        }

        public bool DrawFromDiscardPile(string accessToken)
        {
            return Call("draw from the discard pile", service => service.DrawFromDiscardPile(accessToken));
        }

        public bool Discard(string accessToken, PlayingCard card)
        {
            return Call("discard the card", service => service.Discard(accessToken, card));
        }

        public bool Knock(string accessToken, PlayingCard discardedCard)
        {
            return Call("knock", service => service.Knock(accessToken, discardedCard));
        }

        public bool Forfeit(string accessToken)
        {
            return Call("forfeit the round", service => service.ForfeitRound(accessToken));
        }

        public void Stop()
        {
            IGameService service = _service;
            if (service == null)
            {
                return;
            }

            _service = null;
            ICommunicationObject channel = (ICommunicationObject)service;
            bool isClosed = TryCall("unsubscribe from the round", () =>
            {
                service.UnsubscribeFromGame();
                channel.Close();
            });
            if (!isClosed)
            {
                channel.Abort();
            }
        }

        public void OnRoundStarted(RoundDetails round)
        {
            _screenContext.Post(state => RoundStarted?.Invoke(this, round), null);
        }

        public void OnRoundChanged(RoundDetails round)
        {
            _screenContext.Post(state => RoundChanged?.Invoke(this, round), null);
        }

        public void OnRoundFinished(RoundSummary summary)
        {
            _screenContext.Post(state => RoundFinished?.Invoke(this, summary), null);
        }

        public void OnOpponentLeft()
        {
            _screenContext.Post(state => OpponentLeft?.Invoke(this, EventArgs.Empty), null);
        }

        private bool Call(string operation, Action<IGameService> call)
        {
            IGameService service = _service;

            return (service != null) && TryCall(operation, () => call(service));
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            _logger.LogWarning("The channel of the round failed, so the moves no longer arrive.");
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
        }
    }
}