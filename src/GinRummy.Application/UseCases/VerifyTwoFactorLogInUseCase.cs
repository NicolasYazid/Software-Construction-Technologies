using System;

using GinRummy.Application.Ports;
using GinRummy.Domain;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Services;

namespace GinRummy.Application.UseCases
{
    /// <summary>
    /// The confirmation of a sign-in with the second factor (CU-02 FA-04): redeems the emailed code with the
    /// checks of CU-09 and, when it is valid, grants the access token as step 10 of CU-02 does.
    /// </summary>
    public class VerifyTwoFactorLogInUseCase : ITwoFactorLogInPort
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly VerificationCodeRedeemer _codeRedeemer;

        /// <summary>
        /// Builds the use case with the collaborators the composition root assembled.
        /// </summary>
        /// <param name="unitOfWorkFactory">Opens the transaction that groups the writes of one confirmation.</param>
        /// <param name="accessPolicy">Decides who may enter an account and grants the access token.</param>
        /// <param name="codeRedeemer">Checks the typed code and records the result in the stored one.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public VerifyTwoFactorLogInUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountAccessPolicy accessPolicy,
            VerificationCodeRedeemer codeRedeemer)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _codeRedeemer = codeRedeemer ?? throw new ArgumentNullException(nameof(codeRedeemer));
        }

        /// <summary>
        /// Completes a sign-in whose password was accepted by redeeming the code emailed to the account.
        /// </summary>
        /// <param name="email">Email of the account, exactly as the player typed it to sign in.</param>
        /// <param name="verificationCode">Code exactly as the player typed it.</param>
        /// <param name="keepSignedIn">Whether the player asked to stay signed in.</param>
        /// <returns>The granted access token.</returns>
        /// <exception cref="RequiredFieldMissingException">Thrown when the email or the code is empty.</exception>
        /// <exception cref="DisallowedCharactersException">Thrown when the email contains forbidden characters.</exception>
        /// <exception cref="VerificationCodeRejectedException">Thrown when the code cannot be redeemed.</exception>
        /// <exception cref="AccountBannedException">Thrown when a ban restricts the account since its password was accepted.</exception>
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
                    // The used attempt is saved before the rejection is reported, so retrying cannot reset the count (CU-09 EX-01).
                    unitOfWork.Commit();
                    throw new VerificationCodeRejectedException("The code of the second factor was rejected.", checkResult);
                }

                _accessPolicy.EnsureAccessAllowed(account, unitOfWork.Moderation.GetBansOf(account.PlayerId));
                AccessToken accessToken = _accessPolicy.GrantAccess(account, keepSignedIn);
                unitOfWork.Commit();
                result = LogInResult.ForSignedIn(account, accessToken);
            }

            return result;
        }

        // Without an account or a code there is nothing to redeem, and the player has to sign in again to get a new code.
        // It is reported as an expired code, which does not reveal whether the email belongs to an account.
        private static VerificationCode FindStoredCode(IVerificationCodeRepository codes, PlayerAccount account)
        {
            VerificationCode storedCode = null;
            if (account != null)
            {
                storedCode = codes.FindMostRecent(account.PlayerId, VerificationPurpose.TwoFactorLogin);
            }

            if (storedCode == null)
            {
                throw new VerificationCodeRejectedException("No code of the second factor exists.", VerificationCodeCheckResult.Expired);
            }

            return storedCode;
        }
    }
}
