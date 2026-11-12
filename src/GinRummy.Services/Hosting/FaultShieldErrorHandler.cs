using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

using GinRummy.Contracts.Faults.Shared;
using GinRummy.Domain;
using GinRummy.Services.Services.Shared;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Hosting
{
    public class FaultShieldErrorHandler : IErrorHandler
    {
        private const string InternalErrorCode = "InternalError";
        private const string InternalErrorReason = "The server could not complete the operation.";
        private const string ServiceFaultActionSuffix = "ServiceFaultFault";

        private readonly ILogger<FaultShieldErrorHandler> _errorLogger;

        public FaultShieldErrorHandler(ILogger<FaultShieldErrorHandler> errorLogger)
        {
            _errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
        }

        public bool HandleError(Exception error)
        {
            if (error is FaultException)
            {
                _errorLogger.LogDebug("A service operation returned a declared fault.");
            }
            else if (error is InvalidAccessTokenException)
            {
                _errorLogger.LogWarning(error, "A client presented an access token that is not valid.");
            }
            else if (error is CommunicationException)
            {
                _errorLogger.LogWarning(error, "A client channel failed or closed while it was in use.");
            }
            else
            {
                _errorLogger.LogError(error, "Unexpected error in a service operation.");
            }

            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (error is InvalidAccessTokenException)
            {
                fault = CreateSessionExpiredFault(version);
            }
            else if (!(error is FaultException))
            {
                FaultException shieldedFault = new FaultException(
                    new FaultReason(InternalErrorReason),
                    new FaultCode(InternalErrorCode));
                MessageFault messageFault = shieldedFault.CreateMessageFault();
                fault = Message.CreateMessage(version, messageFault, shieldedFault.Action);
            }
        }

        private static Message CreateSessionExpiredFault(MessageVersion version)
        {
            FaultException<ServiceFault> sessionExpiredFault = ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.SessionExpired);
            MessageFault messageFault = sessionExpiredFault.CreateMessageFault();
            string faultAction = OperationContext.Current.IncomingMessageHeaders.Action + ServiceFaultActionSuffix;

            return Message.CreateMessage(version, messageFault, faultAction);
        }
    }
}
