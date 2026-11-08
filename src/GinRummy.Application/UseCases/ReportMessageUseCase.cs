using System;

using GinRummy.Application.Ports;
using GinRummy.Domain.Entities.PlayerModeration;
using GinRummy.Domain.Entities.SocialCommunication;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Services;

namespace GinRummy.Application.UseCases
{
    public class ReportMessageUseCase : IMessageReportPort
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly IReportedMessageSource _reportedMessageSource;

        public ReportMessageUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountAccessPolicy accessPolicy,
            IReportedMessageSource reportedMessageSource)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _reportedMessageSource = reportedMessageSource ?? throw new ArgumentNullException(nameof(reportedMessageSource));
        }

        public bool ReportMessage(string accessToken, int chatMessageId)
        {
            int reporterPlayerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            Message reportedMessage = _reportedMessageSource.SaveReportedMessage(chatMessageId);
            bool isReported = false;
            if (reportedMessage != null)
            {
                using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
                {
                    Report newReport = new Report(reporterPlayerId, reportedMessage.PlayerId, reportedMessage.MessageId);
                    unitOfWork.Moderation.AddReport(newReport);
                    unitOfWork.Commit();
                }

                isReported = true;
            }

            return isReported;
        }
    }
}
