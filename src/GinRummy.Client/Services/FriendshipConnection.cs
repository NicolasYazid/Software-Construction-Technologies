using System;
using System.ServiceModel;
using System.Threading;

using GinRummy.Client.FriendshipServiceReference;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.Services
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public sealed class FriendshipConnection : IFriendshipServiceCallback
    {
        private readonly Func<InstanceContext, IFriendshipService> _friendshipServiceFactory;
        private readonly ILogger<FriendshipConnection> _logger;
        private readonly SynchronizationContext _screenContext;
        private IFriendshipService _friendshipService;

        public FriendshipConnection(
            Func<InstanceContext, IFriendshipService> friendshipServiceFactory,
            ILogger<FriendshipConnection> logger)
        {
            _friendshipServiceFactory = friendshipServiceFactory ?? throw new ArgumentNullException(nameof(friendshipServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _screenContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public event EventHandler<FriendRequest> FriendRequestReceived;

        public bool Start(string accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            bool isSubscribed = false;
            IFriendshipService friendshipService = _friendshipServiceFactory(new InstanceContext(this));
            ICommunicationObject communicationObject = (ICommunicationObject)friendshipService;
            try
            {
                friendshipService.SubscribeToFriendship(accessToken);
                communicationObject.Faulted += OnChannelFaulted;
                _friendshipService = friendshipService;
                isSubscribed = true;
            }
            catch (FaultException<ServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogWarning(
                    ex,
                    "The server refused to subscribe the player to the friendship notices with the fault {FaultCode}.",
                    ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not subscribe the player to the friendship notices.");
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The friendship service could not be reached to subscribe the player.");
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The friendship service timed out while subscribing the player.");
            }

            return isSubscribed;
        }

        public FriendRequestOutcome? SendFriendRequest(string accessToken, int addresseePublicTag)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            FriendRequestOutcome? outcome = null;
            if (_friendshipService != null)
            {
                outcome = CallSendFriendRequest(accessToken, addresseePublicTag);
            }

            return outcome;
        }

        public void Stop()
        {
            if (_friendshipService == null)
            {
                return;
            }

            IFriendshipService friendshipService = _friendshipService;
            _friendshipService = null;
            ICommunicationObject communicationObject = (ICommunicationObject)friendshipService;
            try
            {
                friendshipService.UnsubscribeFromFriendship();
                communicationObject.Close();
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogWarning(ex, "The channel of the friendship notices did not close cleanly.");
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogWarning(ex, "The channel of the friendship notices took too long to close.");
            }
        }

        public void OnFriendRequestReceived(FriendRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _screenContext.Post(state => FriendRequestReceived?.Invoke(this, request), null);
        }

        private FriendRequestOutcome? CallSendFriendRequest(string accessToken, int addresseePublicTag)
        {
            FriendRequestOutcome? outcome = null;
            try
            {
                outcome = _friendshipService.SendFriendRequest(accessToken, addresseePublicTag);
            }
            catch (FaultException<ServiceFault> ex)
            {
                _logger.LogWarning(ex, "The server refused the friend request with the fault {FaultCode}.", ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                _logger.LogError(ex, "The server could not send the friend request.");
            }
            catch (CommunicationException ex)
            {
                _logger.LogError(ex, "The friendship service could not be reached to send the friend request.");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "The friendship service timed out while sending the friend request.");
            }

            return outcome;
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            _logger.LogWarning("The channel of the friendship notices failed, so the new friend requests no longer arrive.");
        }
    }
}
