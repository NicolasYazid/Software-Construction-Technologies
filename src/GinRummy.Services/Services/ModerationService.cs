using System;
using System.ServiceModel;

using GinRummy.Application.Ports.PlayerModeration;
using GinRummy.Contracts.Faults;
using GinRummy.Contracts.Services;
using GinRummy.Domain;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class ModerationService : IModerationService
    {
        private readonly IMessageReportPort _messageReportPort;
        private readonly ILogger<ModerationService> _logger;

        public ModerationService(IMessageReportPort messageReportPort, ILogger<ModerationService> logger)
        {
            _messageReportPort = messageReportPort ?? throw new ArgumentNullException(nameof(messageReportPort));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void ReportMessage(string accessToken, int chatMessageId)
        {
            bool isReported;
            try
            {
                isReported = _messageReportPort.ReportMessage(accessToken, chatMessageId);
            }
            catch (InvalidAccessTokenException ex)
            {
                _logger.LogWarning(ex, "A client tried to report a message with an access token that is not valid.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.SessionExpired);
            }

            if (!isReported)
            {
                _logger.LogInformation("The chat no longer keeps the message {ChatMessageId}.", chatMessageId);
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.MessageNotFound);
            }
        }
    }
}
