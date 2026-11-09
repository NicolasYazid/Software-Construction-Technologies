using System;

using GinRummy.Application.Ports.AccountProtection;
using GinRummy.Domain;
using GinRummy.Domain.Entities.AccountProtection;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases.AccountProtection
{
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
            int playerAccountId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            PlayerAccount account;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                account = unitOfWork.PlayerAccounts.FindByPlayerAccountId(playerAccountId);
            }

            return account;
        }

        public void RequestTwoFactorActivation(string accessToken)
        {
            int playerAccountId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByPlayerAccountId(playerAccountId);
                CommitSentCode(unitOfWork, _codeSender.Send(unitOfWork, account, VerificationPurpose.TwoFactorSetup));
            }
        }

        public void DisableTwoFactor(string accessToken, string password)
        {
            int playerAccountId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            TypedFieldValidator.EnsureAreTyped(password);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByPlayerAccountId(playerAccountId);
                _accessPolicy.EnsureCredentialsMatch(account, password);
                account.DisableTwoFactor();
                unitOfWork.Commit();
            }
        }

        public void RequestPasswordChange(string accessToken, string currentPassword, string newPassword)
        {
            int playerAccountId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            TypedFieldValidator.EnsureAreTyped(currentPassword, newPassword);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByPlayerAccountId(playerAccountId);
                _accessPolicy.EnsureCredentialsMatch(account, currentPassword);
                _accessPolicy.EnsurePasswordCanReplace(account, newPassword);
                CommitSentCode(unitOfWork, _codeSender.Send(unitOfWork, account, VerificationPurpose.PasswordChange));
            }
        }

        public void RequestEmailChange(string accessToken, string newEmail)
        {
            int playerAccountId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
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

                PlayerAccount account = unitOfWork.PlayerAccounts.FindByPlayerAccountId(playerAccountId);
                CommitSentCode(unitOfWork, _codeSender.SendToNewEmail(unitOfWork, account, normalizedEmail));
            }
        }

        public void ChangeChatFilter(string accessToken, bool isChatFilterEnabled)
        {
            int playerAccountId = _accessPolicy.IdentifyPlayerByAccessToken(accessToken);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                unitOfWork.PlayerPreferences.FindById(playerAccountId).ChangeChatFilter(isChatFilterEnabled);
                unitOfWork.Commit();
            }
        }

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
