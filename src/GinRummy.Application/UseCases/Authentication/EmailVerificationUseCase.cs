using System;

using GinRummy.Application.Ports.Authentication;
using GinRummy.Domain;
using GinRummy.Domain.Entities.AccountProtection;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Ports.AccountProtection;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.Authentication
{
    public class EmailVerificationUseCase : IEmailVerificationPort
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly VerificationCodeRedeemer _codeRedeemer;
        private readonly VerificationCodeSender _codeSender;

        public EmailVerificationUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            VerificationCodeRedeemer codeRedeemer,
            VerificationCodeSender codeSender)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _codeRedeemer = codeRedeemer ?? throw new ArgumentNullException(nameof(codeRedeemer));
            _codeSender = codeSender ?? throw new ArgumentNullException(nameof(codeSender));
        }

        public void VerifyEmail(string email, string verificationCode)
        {
            TypedFieldValidator.EnsureAreTyped(email, verificationCode);
            TypedFieldValidator.EnsureHaveNoForbiddenCharacters(email);

            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByEmail(PlayerAccount.NormalizeEmail(email));
                EnsureIsNotVerified(account);
                VerificationCode storedCode = FindStoredCode(unitOfWork.VerificationCodes, account);
                VerificationCodeCheckResult checkResult = _codeRedeemer.Redeem(unitOfWork.VerificationCodes, storedCode, verificationCode);
                if (checkResult != VerificationCodeCheckResult.Valid)
                {
                    unitOfWork.Commit();
                    throw new VerificationCodeRejectedException("The code that verifies the email was rejected.", checkResult);
                }

                account.MarkEmailVerified();
                unitOfWork.Commit();
            }
        }

        public void ResendVerificationCode(string email)
        {
            TypedFieldValidator.EnsureAreTyped(email);
            TypedFieldValidator.EnsureHaveNoForbiddenCharacters(email);

            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByEmail(PlayerAccount.NormalizeEmail(email));
                if (account != null)
                {
                    SendNewCode(unitOfWork, account);
                }
            }
        }

        private void SendNewCode(IUnitOfWork unitOfWork, PlayerAccount account)
        {
            EnsureIsNotVerified(account);
            EmailDeliveryStatus deliveryStatus = _codeSender.Send(unitOfWork, account, VerificationPurpose.CreateAccount);
            unitOfWork.Commit();
            if (deliveryStatus == EmailDeliveryStatus.Failed)
            {
                throw new VerificationEmailNotSentException("The code that verifies the email could not be emailed.");
            }
        }

        private static void EnsureIsNotVerified(PlayerAccount account)
        {
            if ((account != null) && account.IsEmailVerified)
            {
                throw new EmailAlreadyVerifiedException("The email of the account was already verified.");
            }
        }

        private static VerificationCode FindStoredCode(IVerificationCodeRepository codes, PlayerAccount account)
        {
            VerificationCode storedCode = null;
            if (account != null)
            {
                storedCode = codes.FindMostRecent(account.PlayerAccountId, VerificationPurpose.CreateAccount);
            }

            if (storedCode == null)
            {
                throw new VerificationCodeRejectedException(
                    "No code that verifies the email exists.",
                    VerificationCodeCheckResult.Expired);
            }

            return storedCode;
        }
    }
}
