using System;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Ports.Infrastructure;
using GinRummy.Domain.Ports.Repositories;

namespace GinRummy.Domain.Services
{
    /// <summary>
    /// Sends a new verification code to the email of an account, in the language of its player, and records
    /// the email with the status the mail service reported. Every operation that confirms itself with a code
    /// goes through here; only the purpose changes the text of the email.
    /// </summary>
    public class VerificationCodeSender
    {
        private readonly VerificationCodeIssuer _codeIssuer;
        private readonly IEmailSender _emailSender;
        private readonly IVerificationEmailComposer _emailComposer;

        /// <summary>
        /// Builds the sender with the issuer, the mail service and the composer the composition root assembled.
        /// </summary>
        /// <param name="codeIssuer">Issues and stores the code.</param>
        /// <param name="emailSender">Delivers the email.</param>
        /// <param name="emailComposer">Writes the subject and the body for each purpose and language.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public VerificationCodeSender(
            VerificationCodeIssuer codeIssuer,
            IEmailSender emailSender,
            IVerificationEmailComposer emailComposer)
        {
            _codeIssuer = codeIssuer ?? throw new ArgumentNullException(nameof(codeIssuer));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _emailComposer = emailComposer ?? throw new ArgumentNullException(nameof(emailComposer));
        }

        /// <summary>
        /// Issues a code for the account and emails it.
        /// </summary>
        /// <param name="unitOfWork">Unit of work of the operation that asks for the code.</param>
        /// <param name="account">Account that receives the code, with its player loaded.</param>
        /// <param name="purpose">Operation the code confirms.</param>
        /// <returns>Sent when the mail service accepted the email; Failed otherwise.</returns>
        public EmailDeliveryStatus Send(IUnitOfWork unitOfWork, PlayerAccount account, VerificationPurpose purpose)
        {
            IssuedVerificationCode issuedCode = _codeIssuer.Issue(unitOfWork.VerificationCodes, account.PlayerId, purpose);
            string localeCode = FindLocaleCode(unitOfWork.Locales, account.Player.LocaleId);
            string subject = _emailComposer.ComposeSubject(purpose, localeCode);
            string body = _emailComposer.ComposeBody(purpose, issuedCode.PlainCode, localeCode);
            EmailDeliveryStatus deliveryStatus = _emailSender.Send(account.Email, subject, body);

            // The email is recorded even when it failed, so a code that never left can be told apart from one that did not arrive.
            // It is dated with the code because both belong to the same request (CU-04 EX-03).
            VerificationCode storedCode = issuedCode.StoredCode;
            Email sentEmail = new Email(storedCode.VerificationCodeId, account.Email, storedCode.CreatedAt, deliveryStatus);
            unitOfWork.VerificationCodes.AddEmail(sentEmail);

            return deliveryStatus;
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
