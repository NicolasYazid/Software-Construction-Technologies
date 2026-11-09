using System;

using GinRummy.Application.Ports.AccountProtection;
using GinRummy.Domain;
using GinRummy.Domain.Entities.AccountProtection;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Ports.AccountProtection;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.AccountProtection
{
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
            int playerAccountId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            TypedFieldValidator.EnsureAreTyped(verificationCode);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                VerificationCode storedCode = FindStoredCode(unitOfWork.VerificationCodes, playerAccountId, VerificationPurpose.TwoFactorSetup);
                RedeemCode(unitOfWork, storedCode, verificationCode);
                unitOfWork.PlayerAccounts.FindByPlayerAccountId(playerAccountId).EnableTwoFactor();
                unitOfWork.Commit();
            }
        }

        public void ConfirmPasswordChange(string accessToken, string newPassword, string verificationCode)
        {
            int playerAccountId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            TypedFieldValidator.EnsureAreTyped(newPassword, verificationCode);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByPlayerAccountId(playerAccountId);
                VerificationCode storedCode = FindStoredCode(unitOfWork.VerificationCodes, playerAccountId, VerificationPurpose.PasswordChange);
                RedeemCode(unitOfWork, storedCode, verificationCode);
                _accessPolicy.ReplacePassword(account, newPassword);
                unitOfWork.Commit();
            }
        }

        public void ConfirmEmailChange(string accessToken, string newEmail, string verificationCode)
        {
            int playerAccountId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            TypedFieldValidator.EnsureAreTyped(newEmail, verificationCode);
            string normalizedEmail = PlayerAccount.NormalizeEmail(newEmail);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                VerificationCode storedCode = FindStoredCode(unitOfWork.VerificationCodes, playerAccountId, VerificationPurpose.EmailChange);
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
                unitOfWork.PlayerAccounts.FindByPlayerAccountId(playerAccountId).ChangeEmail(normalizedEmail);
                unitOfWork.Commit();
            }
        }

        private static VerificationCode FindStoredCode(IVerificationCodeRepository codes, int playerAccountId, VerificationPurpose purpose)
        {
            VerificationCode storedCode = codes.FindMostRecent(playerAccountId, purpose);
            if (storedCode == null)
            {
                throw new VerificationCodeRejectedException("No code of the account change exists.", VerificationCodeCheckResult.Expired);
            }

            return storedCode;
        }

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
