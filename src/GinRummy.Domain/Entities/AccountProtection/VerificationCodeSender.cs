using System;

using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerPreferences;
using GinRummy.Domain.Ports.AccountProtection;
using GinRummy.Domain.Ports.PlayerPreferences;
using GinRummy.Domain.Shared;

namespace GinRummy.Domain.Entities.AccountProtection
{
    public class VerificationCodeSender
    {
        private readonly VerificationCodeIssuer _codeIssuer;
        private readonly IEmailSender _emailSender;
        private readonly IVerificationEmailComposer _emailComposer;

        public VerificationCodeSender(
            VerificationCodeIssuer codeIssuer,
            IEmailSender emailSender,
            IVerificationEmailComposer emailComposer)
        {
            _codeIssuer = codeIssuer ?? throw new ArgumentNullException(nameof(codeIssuer));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _emailComposer = emailComposer ?? throw new ArgumentNullException(nameof(emailComposer));
        }

        public EmailDeliveryStatus Send(IUnitOfWork unitOfWork, PlayerAccount account, VerificationPurpose purpose)
        {
            IssuedVerificationCode issuedCode = _codeIssuer.Issue(unitOfWork.VerificationCodes, account.PlayerAccountId, purpose);
            string localeCode = FindLocaleCode(unitOfWork.Locales, account.PlayerPreferences.LocaleId);
            Email sentEmail = Deliver(issuedCode, account.Email, localeCode);
            unitOfWork.VerificationCodes.AddEmail(sentEmail);

            return sentEmail.DeliveryStatus;
        }

        public EmailDeliveryStatus SendToNewEmail(IUnitOfWork unitOfWork, PlayerAccount account, string newEmail)
        {
            IssuedVerificationCode issuedCode = _codeIssuer.Issue(
                unitOfWork.VerificationCodes,
                account.PlayerAccountId,
                VerificationPurpose.EmailChange);
            string localeCode = FindLocaleCode(unitOfWork.Locales, account.PlayerPreferences.LocaleId);
            Email sentEmail = Deliver(issuedCode, newEmail, localeCode);
            unitOfWork.VerificationCodes.AddEmail(sentEmail);

            return sentEmail.DeliveryStatus;
        }

        private Email Deliver(IssuedVerificationCode issuedCode, string destinationAddress, string localeCode)
        {
            VerificationCode storedCode = issuedCode.StoredCode;
            string subject = _emailComposer.ComposeSubject(storedCode.Purpose, localeCode);
            string body = _emailComposer.ComposeBody(storedCode.Purpose, issuedCode.PlainCode, localeCode);
            EmailDeliveryStatus deliveryStatus = _emailSender.Send(destinationAddress, subject, body);

            return new Email(storedCode.VerificationCodeId, destinationAddress, storedCode.CreatedAt, deliveryStatus);
        }

        private static string FindLocaleCode(ILocaleRepository locales, int localeId)
        {
            string localeCode = string.Empty;
            Locale locale = locales.FindById(localeId);
            if (locale != null)
            {
                localeCode = locale.LocaleCode;
            }

            return localeCode;
        }
    }
}
