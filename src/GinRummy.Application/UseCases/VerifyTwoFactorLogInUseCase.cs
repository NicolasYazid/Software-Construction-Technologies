using System;

using GinRummy.Application.Ports;
using GinRummy.Domain;
using GinRummy.Domain.Entities.AccountProtection;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Ports.AccountProtection;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases
{
    public class VerifyTwoFactorLogInUseCase : ITwoFactorLogInPort
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly VerificationCodeRedeemer _codeRedeemer;

        public VerifyTwoFactorLogInUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountAccessPolicy accessPolicy,
            VerificationCodeRedeemer codeRedeemer)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _codeRedeemer = codeRedeemer ?? throw new ArgumentNullException(nameof(codeRedeemer));
        }

        public LogInResult VerifyTwoFactorLogIn(string email, string verificationCode, bool keepSignedIn)
        {
            TypedFieldValidator.EnsureAreTyped(email, verificationCode);
            TypedFieldValidator.EnsureHaveNoForbiddenCharacters(email);

            LogInResult result;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByEmail(PlayerAccount.NormalizeEmail(email));
                VerificationCode storedCode = FindStoredCode(unitOfWork.VerificationCodes, account);
                VerificationCodeCheckResult checkResult = _codeRedeemer.Redeem(unitOfWork.VerificationCodes, storedCode, verificationCode);
                if (checkResult != VerificationCodeCheckResult.Valid)
                {
                    unitOfWork.Commit();
                    throw new VerificationCodeRejectedException("The code of the second factor was rejected.", checkResult);
                }

                _accessPolicy.EnsureAccessAllowed(account, unitOfWork.Moderation.GetBansOf(account.PlayerAccountId));
                AccessToken accessToken = _accessPolicy.GrantAccess(account, keepSignedIn);
                unitOfWork.Commit();
                result = LogInResult.ForSignedIn(account, accessToken);
            }

            return result;
        }

        private static VerificationCode FindStoredCode(IVerificationCodeRepository codes, PlayerAccount account)
        {
            VerificationCode storedCode = null;
            if (account != null)
            {
                storedCode = codes.FindMostRecent(account.PlayerAccountId, VerificationPurpose.TwoFactorLogin);
            }

            if (storedCode == null)
            {
                throw new VerificationCodeRejectedException("No code of the second factor exists.", VerificationCodeCheckResult.Expired);
            }

            return storedCode;
        }
    }
}
