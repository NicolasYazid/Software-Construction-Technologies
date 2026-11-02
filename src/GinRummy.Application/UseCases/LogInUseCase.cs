using System;

using GinRummy.Application.Ports;
using GinRummy.Domain;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Services;

namespace GinRummy.Application.UseCases
{
    /// <summary>
    /// The Log In use case (CU-02): validates what the player typed, checks the credentials and the state of
    /// the account, and either grants the access token or emails the code of the second factor.
    /// </summary>
    public class LogInUseCase : ILogInPort
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly VerificationCodeSender _codeSender;

        /// <summary>
        /// Builds the use case with the collaborators the composition root assembled.
        /// </summary>
        /// <param name="unitOfWorkFactory">Opens the transaction that groups the writes of one sign-in.</param>
        /// <param name="accessPolicy">Decides who may enter an account and grants the access token.</param>
        /// <param name="codeSender">Emails the code of the second factor.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public LogInUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountAccessPolicy accessPolicy,
            VerificationCodeSender codeSender)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _codeSender = codeSender ?? throw new ArgumentNullException(nameof(codeSender));
        }

        /// <summary>
        /// Signs a player in with the email and the password of the account.
        /// </summary>
        /// <param name="email">Email exactly as the player typed it.</param>
        /// <param name="password">Password exactly as the player typed it.</param>
        /// <param name="keepSignedIn">Whether the player asked to stay signed in.</param>
        /// <returns>The granted access token, or the notice that the second factor is pending.</returns>
        /// <exception cref="RequiredFieldMissingException">Thrown when the email or the password is empty.</exception>
        /// <exception cref="DisallowedCharactersException">Thrown when a field contains forbidden characters.</exception>
        /// <exception cref="InvalidCredentialsException">Thrown when the email or the password is not valid.</exception>
        /// <exception cref="EmailNotVerifiedException">Thrown when the email of the account is not verified.</exception>
        /// <exception cref="AccountBannedException">Thrown when a ban restricts the whole account.</exception>
        /// <exception cref="VerificationEmailNotSentException">Thrown when the code of the second factor could not be emailed.</exception>
        public LogInResult LogIn(string email, string password, bool keepSignedIn)
        {
            TypedFieldValidator.EnsureAreTyped(email, password);
            TypedFieldValidator.EnsureHaveNoForbiddenCharacters(email, password);

            LogInResult result;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByEmail(PlayerAccount.NormalizeEmail(email));
                _accessPolicy.EnsureCredentialsMatch(account, password);
                _accessPolicy.EnsureAccessAllowed(account, unitOfWork.Moderation.GetBansOf(account.PlayerId));
                if (account.IsTwoFactorEnabled)
                {
                    result = StartTwoFactor(unitOfWork, account);
                }
                else
                {
                    AccessToken accessToken = _accessPolicy.GrantAccess(account, keepSignedIn);
                    unitOfWork.Commit();
                    result = LogInResult.ForSignedIn(account, accessToken);
                }
            }

            return result;
        }

        // The code and its failed email are saved before the failure is reported, so the player can ask for a new code (CU-04 EX-03).
        // The token is not granted here: it waits for the code, as CU-02 FA-04 returns to step 10 only after the check.
        private LogInResult StartTwoFactor(IUnitOfWork unitOfWork, PlayerAccount account)
        {
            EmailDeliveryStatus deliveryStatus = _codeSender.Send(unitOfWork, account, VerificationPurpose.TwoFactorLogin);
            unitOfWork.Commit();
            if (deliveryStatus == EmailDeliveryStatus.Failed)
            {
                throw new VerificationEmailNotSentException("The code of the second factor could not be emailed.");
            }

            return LogInResult.ForPendingTwoFactor(account);
        }
    }
}
