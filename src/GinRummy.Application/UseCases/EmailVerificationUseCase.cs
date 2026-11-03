using System;

using GinRummy.Application.Ports;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.Players;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Services;

namespace GinRummy.Application.UseCases
{
    /// <summary>
    /// The Verify email address use case (CU-09): redeems the code emailed to a new account and marks its
    /// email as verified, or emails a new code when the player asks for one (FA-01).
    /// </summary>
    public class EmailVerificationUseCase : IEmailVerificationPort
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly VerificationCodeRedeemer _codeRedeemer;
        private readonly VerificationCodeSender _codeSender;

        /// <summary>
        /// Builds the use case with the collaborators the composition root assembled.
        /// </summary>
        /// <param name="unitOfWorkFactory">Opens the transaction that groups the writes of one request.</param>
        /// <param name="codeRedeemer">Checks the typed code and records the result in the stored one.</param>
        /// <param name="codeSender">Emails a new code when the player asks for one.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public EmailVerificationUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            VerificationCodeRedeemer codeRedeemer,
            VerificationCodeSender codeSender)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _codeRedeemer = codeRedeemer ?? throw new ArgumentNullException(nameof(codeRedeemer));
            _codeSender = codeSender ?? throw new ArgumentNullException(nameof(codeSender));
        }

        /// <summary>
        /// Verifies the email of an account by redeeming the code emailed to it.
        /// </summary>
        /// <param name="email">Email of the account, exactly as the client has it.</param>
        /// <param name="verificationCode">Code exactly as the player typed it.</param>
        /// <exception cref="RequiredFieldMissingException">Thrown when the email or the code is empty.</exception>
        /// <exception cref="DisallowedCharactersException">Thrown when the email contains forbidden characters.</exception>
        /// <exception cref="EmailAlreadyVerifiedException">Thrown when the email of the account was already verified.</exception>
        /// <exception cref="VerificationCodeRejectedException">Thrown when the code cannot be redeemed.</exception>
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
                    // The used attempt is saved before the rejection is reported, so retrying cannot reset the count (CU-09 EX-01).
                    unitOfWork.Commit();
                    throw new VerificationCodeRejectedException("The code that verifies the email was rejected.", checkResult);
                }

                account.MarkEmailVerified();
                unitOfWork.Commit();
            }
        }

        /// <summary>
        /// Emails a new code to an account whose email is not verified yet, replacing the pending one.
        /// </summary>
        /// <param name="email">Email of the account, exactly as the client has it.</param>
        /// <exception cref="RequiredFieldMissingException">Thrown when the email is empty.</exception>
        /// <exception cref="DisallowedCharactersException">Thrown when the email contains forbidden characters.</exception>
        /// <exception cref="EmailAlreadyVerifiedException">Thrown when the email of the account was already verified.</exception>
        /// <exception cref="VerificationResendLimitException">Thrown when the limit of codes does not allow a new one yet.</exception>
        /// <exception cref="VerificationEmailNotSentException">Thrown when the new code could not be emailed.</exception>
        public void ResendVerificationCode(string email)
        {
            TypedFieldValidator.EnsureAreTyped(email);
            TypedFieldValidator.EnsureHaveNoForbiddenCharacters(email);

            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                // The client only asks with the email it just registered or signed in with.
                // An email without an account can only come from a call made by hand, so there is nothing to send.
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByEmail(PlayerAccount.NormalizeEmail(email));
                if (account != null)
                {
                    SendNewCode(unitOfWork, account);
                }
            }
        }

        // The new code and its failed email are saved before the failure is reported, so the player can ask again (CU-09 EX-09).
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

        // Without an account or a code there is nothing to redeem, so it is reported as an expired code that a resend replaces.
        private static VerificationCode FindStoredCode(IVerificationCodeRepository codes, PlayerAccount account)
        {
            VerificationCode storedCode = null;
            if (account != null)
            {
                storedCode = codes.FindMostRecent(account.PlayerId, VerificationPurpose.CreateAccount);
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
