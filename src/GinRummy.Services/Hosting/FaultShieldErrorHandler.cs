using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Hosting
{
    /// <summary>
    /// Last line of defense of every service. It logs the errors the services did not turn into
    /// a declared fault and replaces them with a generic fault, so the client never receives
    /// stack traces or internal messages.
    /// </summary>
    public class FaultShieldErrorHandler : IErrorHandler
    {
        private const string InternalErrorCode = "InternalError";
        private const string InternalErrorReason = "The server could not complete the operation.";

        private readonly ILogger<FaultShieldErrorHandler> _errorLogger;

        /// <summary>
        /// Builds the handler with the logger it writes to.
        /// </summary>
        /// <param name="errorLogger">Logger of the server.</param>
        public FaultShieldErrorHandler(ILogger<FaultShieldErrorHandler> errorLogger)
        {
            _errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
        }

        /// <summary>
        /// Logs the error after the reply was sent. Declared faults are part of the normal flow,
        /// so they are not logged as errors.
        /// </summary>
        /// <param name="error">Error raised by the operation or the channel.</param>
        /// <returns>
        /// Always true, so WCF does not abort the session of the client for an error the
        /// handler already dealt with.
        /// </returns>
        public bool HandleError(Exception error)
        {
            if (error is FaultException)
            {
                _errorLogger.LogDebug("A service operation returned a declared fault.");
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

        /// <summary>
        /// Replaces every error that is not a declared fault with a generic fault without
        /// internal details.
        /// </summary>
        /// <param name="error">Error raised by the operation.</param>
        /// <param name="version">Message version of the channel.</param>
        /// <param name="fault">Fault sent to the client; left null to keep the declared fault.</param>
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (!(error is FaultException))
            {
                FaultException shieldedFault = new FaultException(
                    new FaultReason(InternalErrorReason),
                    new FaultCode(InternalErrorCode));
                MessageFault messageFault = shieldedFault.CreateMessageFault();
                fault = Message.CreateMessage(version, messageFault, shieldedFault.Action);
            }
        }
    }
}
