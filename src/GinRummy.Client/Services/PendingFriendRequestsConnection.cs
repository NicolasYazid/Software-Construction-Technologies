using System;
using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Client.Models;
using GinRummy.Client.PendingFriendRequestServiceReference;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.Services
{
    public class PendingFriendRequestsConnection
    {
        private readonly Func<IPendingFriendRequestService> _pendingFriendRequestServiceFactory;
        private readonly string _accessToken;
        private readonly ILogger<PendingFriendRequestsConnection> _logger;

        public PendingFriendRequestsConnection(
            Func<IPendingFriendRequestService> pendingFriendRequestServiceFactory,
            string accessToken,
            ILogger<PendingFriendRequestsConnection> logger)
        {
            _pendingFriendRequestServiceFactory = pendingFriendRequestServiceFactory
                ?? throw new ArgumentNullException(nameof(pendingFriendRequestServiceFactory));
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public event EventHandler<ErrorDialogKind> RequestsUnavailable;

        public IList<FriendRequest> GetPendingFriendRequests()
        {
            IList<FriendRequest> pendingRequests = new List<FriendRequest>();
            IPendingFriendRequestService pendingFriendRequestService = _pendingFriendRequestServiceFactory();
            ICommunicationObject communicationObject = (ICommunicationObject)pendingFriendRequestService;
            try
            {
                pendingRequests = pendingFriendRequestService.GetPendingFriendRequests(_accessToken);
                communicationObject.Close();
            }
            catch (FaultException<ServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogWarning(ex, "The server refused the pending friend requests with the fault {FaultCode}.", ex.Detail.Code);
                NotifyRequestsUnavailable(FindErrorDialogKind(ex.Detail.Code));
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not read the pending friend requests.");
                NotifyRequestsUnavailable(ErrorDialogKind.ServiceUnavailable);
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The pending friend request service could not be reached.");
                NotifyRequestsUnavailable(ErrorDialogKind.ConnectionLost);
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The pending friend request service timed out.");
                NotifyRequestsUnavailable(ErrorDialogKind.ConnectionLost);
            }

            return pendingRequests;
        }

        private static ErrorDialogKind FindErrorDialogKind(ServiceFaultCode faultCode)
        {
            ErrorDialogKind errorDialogKind = ErrorDialogKind.ServiceUnavailable;
            if (faultCode == ServiceFaultCode.SessionExpired)
            {
                errorDialogKind = ErrorDialogKind.SessionExpired;
            }

            return errorDialogKind;
        }

        private void NotifyRequestsUnavailable(ErrorDialogKind errorDialogKind)
        {
            RequestsUnavailable?.Invoke(this, errorDialogKind);
        }
    }
}
