using System;
using System.ServiceModel;

using GinRummy.Client.FriendRequestResponseServiceReference;
using GinRummy.Client.Models;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.Services
{
    public class FriendRequestResponseConnection
    {
        private readonly Func<IFriendRequestResponseService> _responseServiceFactory;
        private readonly string _accessToken;
        private readonly ILogger<FriendRequestResponseConnection> _logger;

        public FriendRequestResponseConnection(
            Func<IFriendRequestResponseService> responseServiceFactory,
            string accessToken,
            ILogger<FriendRequestResponseConnection> logger)
        {
            _responseServiceFactory = responseServiceFactory ?? throw new ArgumentNullException(nameof(responseServiceFactory));
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public event EventHandler<ErrorDialogKind> ResponseUnavailable;

        public FriendRequestResponseOutcome? AcceptFriendRequest(int friendRequestId)
        {
            return CallResponseService(responseService => responseService.AcceptFriendRequest(_accessToken, friendRequestId));
        }

        public FriendRequestResponseOutcome? DeclineFriendRequest(int friendRequestId)
        {
            return CallResponseService(responseService => responseService.DeclineFriendRequest(_accessToken, friendRequestId));
        }

        private FriendRequestResponseOutcome? CallResponseService(Func<IFriendRequestResponseService, FriendRequestResponseOutcome> respond)
        {
            FriendRequestResponseOutcome? outcome = null;
            IFriendRequestResponseService responseService = _responseServiceFactory();
            ICommunicationObject communicationObject = (ICommunicationObject)responseService;
            try
            {
                outcome = respond(responseService);
                communicationObject.Close();
            }
            catch (FaultException<ServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogWarning(ex, "The server refused the answer to the friend request with the fault {FaultCode}.", ex.Detail.Code);
                NotifyResponseUnavailable(FindErrorDialogKind(ex.Detail.Code));
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not answer the friend request.");
                NotifyResponseUnavailable(ErrorDialogKind.ServiceUnavailable);
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The friend request response service could not be reached.");
                NotifyResponseUnavailable(ErrorDialogKind.ConnectionLost);
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The friend request response service timed out.");
                NotifyResponseUnavailable(ErrorDialogKind.ConnectionLost);
            }

            return outcome;
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

        private void NotifyResponseUnavailable(ErrorDialogKind errorDialogKind)
        {
            ResponseUnavailable?.Invoke(this, errorDialogKind);
        }
    }
}
