using System;

using GinRummy.Application.Ports;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.Players;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Services;

namespace GinRummy.Application.UseCases
{
    // A change of the account is applied only with the code emailed for it, so a session alone cannot change it (CU-04, CU-06, CU-07).
    public class ConfirmAccountChangeUseCase : IAccountChangeConfirmationPort
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly VerificationCodeRedeemer _codeRedeemer;

        public ConfirmAccountChangeUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountAccessPolicy accessPolicy,
            VerificationCodeRedeemer codeRedeemer)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _codeRedeemer = codeRedeemer ?? throw new ArgumentNullException(nameof(codeRedeemer));
        }

        public void ConfirmTwoFactorActivation(string accessToken, string verificationCode)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            TypedFieldValidator.EnsureAreTyped(verificationCode);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                VerificationCode storedCode = FindStoredCode(unitOfWork.VerificationCodes, playerId, VerificationPurpose.TwoFactorSetup);
                RedeemCode(unitOfWork, storedCode, verificationCode);
                unitOfWork.PlayerAccounts.FindByPlayerId(playerId).EnableTwoFactor();
                unitOfWork.Commit();
            }
        }

        // The new password is checked before the code is redeemed, so a weak password does not use up the code (CU-06).
        public void ConfirmPasswordChange(string accessToken, string newPassword, string verificationCode)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            TypedFieldValidator.EnsureAreTyped(newPassword, verificationCode);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByPlayerId(playerId);
                _accessPolicy.EnsurePasswordCanReplace(account, newPassword);
                VerificationCode storedCode = FindStoredCode(unitOfWork.VerificationCodes, playerId, VerificationPurpose.PasswordChange);
                RedeemCode(unitOfWork, storedCode, verificationCode);
                _accessPolicy.ReplacePassword(account, newPassword);
                unitOfWork.Commit();
            }
        }

        // Only the address the code was emailed to is kept, so an address nobody proved to reach never replaces the email (CU-07).
        // Another account may have taken the address while the code was on its way, so the address is checked again.
        public void ConfirmEmailChange(string accessToken, string newEmail, string verificationCode)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            TypedFieldValidator.EnsureAreTyped(newEmail, verificationCode);
            string normalizedEmail = PlayerAccount.NormalizeEmail(newEmail);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                VerificationCode storedCode = FindStoredCode(unitOfWork.VerificationCodes, playerId, VerificationPurpose.EmailChange);
                Email sentEmail = unitOfWork.VerificationCodes.FindEmailOf(storedCode.VerificationCodeId);
                if ((sentEmail == null) || (sentEmail.DestinationAddress != normalizedEmail))
                {
                    throw new VerificationCodeRejectedException(
                        "The code was emailed to another address.",
                        VerificationCodeCheckResult.Incorrect);
                }

                if (unitOfWork.PlayerAccounts.FindByEmail(normalizedEmail) != null)
                {
                    throw new EmailAlreadyRegisteredException("The new email already belongs to an account.");
                }

                RedeemCode(unitOfWork, storedCode, verificationCode);
                unitOfWork.PlayerAccounts.FindByPlayerId(playerId).ChangeEmail(normalizedEmail);
                unitOfWork.Commit();
            }
        }

        // Without a code there is nothing to redeem, and the player has to ask for a new one.
        private static VerificationCode FindStoredCode(IVerificationCodeRepository codes, int playerId, VerificationPurpose purpose)
        {
            VerificationCode storedCode = codes.FindMostRecent(playerId, purpose);
            if (storedCode == null)
            {
                throw new VerificationCodeRejectedException("No code of the account change exists.", VerificationCodeCheckResult.Expired);
            }

            return storedCode;
        }

        // The used attempt is saved before the rejection is reported, so retrying cannot reset the count (CU-09 EX-01).
        private void RedeemCode(IUnitOfWork unitOfWork, VerificationCode storedCode, string typedCode)
        {
            VerificationCodeCheckResult checkResult = _codeRedeemer.Redeem(unitOfWork.VerificationCodes, storedCode, typedCode);
            if (checkResult != VerificationCodeCheckResult.Valid)
            {
                unitOfWork.Commit();
                throw new VerificationCodeRejectedException("The code of the account change was rejected.", checkResult);
            }
        }
    }
}
