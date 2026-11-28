using System;

using GinRummy.Application.Ports.Authentication;
using GinRummy.Domain;
using GinRummy.Domain.Entities.AccountProtection;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Ports.AccountProtection;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.Authentication
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

        public LogInResult VerifyTwoFactorLogIn(string email, string verificationCode, bool isKeepSignedIn)
        {
            LogInResult result;
            if (!TypedFieldValidator.AreTyped(email, verificationCode))
            {
                result = LogInResult.ForRejected(LogInStatus.RequiredField);
            }
            else if (TypedFieldValidator.HaveForbiddenCharacters(email))
            {
                result = LogInResult.ForRejected(LogInStatus.DisallowedCharacters);
            }
            else
            {
                result = RedeemCode(email, verificationCode, isKeepSignedIn);
            }

            return result;
        }

        private LogInResult RedeemCode(string email, string verificationCode, bool isKeepSignedIn)
        {
            LogInResult result;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByEmail(PlayerAccount.NormalizeEmail(email));
                VerificationCode storedCode = FindStoredCode(unitOfWork.VerificationCodes, account);
                VerificationCodeCheckResult checkResult = VerificationCodeCheckResult.Expired;
                if (storedCode != null)
                {
                    checkResult = _codeRedeemer.Redeem(unitOfWork.VerificationCodes, storedCode, verificationCode);
                }

                if (checkResult != VerificationCodeCheckResult.Valid)
                {
                    unitOfWork.Commit();
                    result = LogInResult.ForRejected(MapRejectedCode(checkResult));
                }
                else
                {
                    result = _accessPolicy.FindAccessRejection(account, unitOfWork.Moderation.GetBansOf(account.PlayerAccountId))
                        ?? GrantAccess(unitOfWork, account, isKeepSignedIn);
                }
            }

            return result;
        }

        private LogInResult GrantAccess(IUnitOfWork unitOfWork, PlayerAccount account, bool isKeepSignedIn)
        {
            AccessToken accessToken = _accessPolicy.GrantAccess(account, isKeepSignedIn);
            unitOfWork.Commit();

            return LogInResult.ForSignedIn(account, accessToken);
        }

        private static VerificationCode FindStoredCode(IVerificationCodeRepository codes, PlayerAccount account)
        {
            VerificationCode storedCode = null;
            if (account != null)
            {
                storedCode = codes.FindMostRecent(account.PlayerAccountId, VerificationPurpose.TwoFactorLogin);
            }

            return storedCode;
        }

        private static LogInStatus MapRejectedCode(VerificationCodeCheckResult checkResult)
        {
            LogInStatus status;
            switch (checkResult)
            {
                case VerificationCodeCheckResult.InvalidFormat:
                    status = LogInStatus.CodeInvalidFormat;
                    break;
                case VerificationCodeCheckResult.AlreadyUsed:
                    status = LogInStatus.CodeAlreadyUsed;
                    break;
                case VerificationCodeCheckResult.Expired:
                    status = LogInStatus.CodeExpired;
                    break;
                case VerificationCodeCheckResult.TooManyAttempts:
                    status = LogInStatus.CodeTooManyAttempts;
                    break;
                default:
                    status = LogInStatus.CodeIncorrect;
                    break;
            }

            return status;
        }
    }
}
