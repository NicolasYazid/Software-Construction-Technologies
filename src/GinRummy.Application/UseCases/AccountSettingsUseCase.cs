using System;

using GinRummy.Application.Ports;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.Players;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Services;

namespace GinRummy.Application.UseCases
{
    // Every operation takes the player from the token, so a player can only change the own account (CU-04 to CU-07).
    public class AccountSettingsUseCase : IAccountSettingsPort
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly VerificationCodeSender _codeSender;

        public AccountSettingsUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountAccessPolicy accessPolicy,
            VerificationCodeSender codeSender)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _codeSender = codeSender ?? throw new ArgumentNullException(nameof(codeSender));
        }

        public PlayerAccount GetAccountSettings(string accessToken)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            PlayerAccount account;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                account = unitOfWork.PlayerAccounts.FindByPlayerId(playerId);
            }

            return account;
        }

        public void RequestTwoFactorActivation(string accessToken)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByPlayerId(playerId);
                CommitSentCode(unitOfWork, _codeSender.Send(unitOfWork, account, VerificationPurpose.TwoFactorSetup));
            }
        }

        // The password is asked again, because an open session alone must not be enough to weaken the account (CU-05).
        public void DisableTwoFactor(string accessToken, string password)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            TypedFieldValidator.EnsureAreTyped(password);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByPlayerId(playerId);
                _accessPolicy.EnsureCredentialsMatch(account, password);
                account.DisableTwoFactor();
                unitOfWork.Commit();
            }
        }

        // Both passwords are checked before the code is sent, so the player learns of a mistake before waiting for an email (CU-06).
        public void RequestPasswordChange(string accessToken, string currentPassword, string newPassword)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            TypedFieldValidator.EnsureAreTyped(currentPassword, newPassword);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByPlayerId(playerId);
                _accessPolicy.EnsureCredentialsMatch(account, currentPassword);
                _accessPolicy.EnsurePasswordCanReplace(account, newPassword);
                CommitSentCode(unitOfWork, _codeSender.Send(unitOfWork, account, VerificationPurpose.PasswordChange));
            }
        }

        // The code goes to the new address, because that is the address that must prove it reaches the player (CU-07).
        public void RequestEmailChange(string accessToken, string newEmail)
        {
            int playerId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            TypedFieldValidator.EnsureAreTyped(newEmail);
            TypedFieldValidator.EnsureHaveNoForbiddenCharacters(newEmail);
            if (PlayerAccount.GetUnmetEmailRequirements(newEmail).Count > 0)
            {
                throw new InvalidEmailFormatException("The new email is not a valid address.");
            }

            string normalizedEmail = PlayerAccount.NormalizeEmail(newEmail);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                if (unitOfWork.PlayerAccounts.FindByEmail(normalizedEmail) != null)
                {
                    throw new EmailAlreadyRegisteredException("The new email already belongs to an account.");
                }

                PlayerAccount account = unitOfWork.PlayerAccounts.FindByPlayerId(playerId);
                CommitSentCode(unitOfWork, _codeSender.SendToNewEmail(unitOfWork, account, normalizedEmail));
            }
        }

        // The code and its failed email are saved before the failure is reported, so the player can ask for a new code (CU-04 EX-03).
        private static void CommitSentCode(IUnitOfWork unitOfWork, EmailDeliveryStatus deliveryStatus)
        {
            unitOfWork.Commit();
            if (deliveryStatus == EmailDeliveryStatus.Failed)
            {
                throw new VerificationEmailNotSentException("The code of the account change could not be emailed.");
            }
        }
    }
}
