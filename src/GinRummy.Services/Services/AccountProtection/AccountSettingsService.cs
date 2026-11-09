using System;
using System.ServiceModel;

using GinRummy.Application.Ports.AccountProtection;
using GinRummy.Contracts.DataContracts.AccountProtection;
using GinRummy.Contracts.Faults.Shared;
using GinRummy.Contracts.Services.AccountProtection;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Services.Services.Shared;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Services.AccountProtection
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class AccountSettingsService : IAccountSettingsService
    {
        private readonly IAccountSettingsPort _accountSettingsPort;
        private readonly IAccountChangeConfirmationPort _confirmationPort;
        private readonly ILogger<AccountSettingsService> _logger;

        public AccountSettingsService(
            IAccountSettingsPort accountSettingsPort,
            IAccountChangeConfirmationPort confirmationPort,
            ILogger<AccountSettingsService> logger)
        {
            _accountSettingsPort = accountSettingsPort ?? throw new ArgumentNullException(nameof(accountSettingsPort));
            _confirmationPort = confirmationPort ?? throw new ArgumentNullException(nameof(confirmationPort));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public AccountSettingsDetails GetAccountSettings(string accessToken)
        {
            PlayerAccount account = null;
            RunAccountChange(() => account = _accountSettingsPort.GetAccountSettings(accessToken));

            return new AccountSettingsDetails
            {
                Email = account.Email,
                IsTwoFactorEnabled = account.IsTwoFactorEnabled,
                IsChatFilterEnabled = account.PlayerPreferences.IsChatFilterEnabled
            };
        }

        public void RequestTwoFactorActivation(string accessToken)
        {
            RunAccountChange(() => _accountSettingsPort.RequestTwoFactorActivation(accessToken));
        }

        public void ConfirmTwoFactorActivation(string accessToken, string verificationCode)
        {
            RunAccountChange(() => _confirmationPort.ConfirmTwoFactorActivation(accessToken, verificationCode));
        }

        public void DisableTwoFactor(string accessToken, string password)
        {
            RunAccountChange(() => _accountSettingsPort.DisableTwoFactor(accessToken, password));
        }

        public void RequestPasswordChange(string accessToken, string currentPassword, string newPassword)
        {
            RunAccountChange(() => _accountSettingsPort.RequestPasswordChange(accessToken, currentPassword, newPassword));
        }

        public void ConfirmPasswordChange(string accessToken, string newPassword, string verificationCode)
        {
            RunAccountChange(() => _confirmationPort.ConfirmPasswordChange(accessToken, newPassword, verificationCode));
        }

        public void RequestEmailChange(string accessToken, string newEmail)
        {
            RunAccountChange(() => _accountSettingsPort.RequestEmailChange(accessToken, newEmail));
        }

        public void ConfirmEmailChange(string accessToken, string newEmail, string verificationCode)
        {
            RunAccountChange(() => _confirmationPort.ConfirmEmailChange(accessToken, newEmail, verificationCode));
        }

        public void ChangeChatFilter(string accessToken, bool isChatFilterEnabled)
        {
            RunAccountChange(() => _accountSettingsPort.ChangeChatFilter(accessToken, isChatFilterEnabled));
        }

        // Every change of the account breaks the same rules, so they all turn each broken rule into the same fault.
        private void RunAccountChange(Action accountChange)
        {
            try
            {
                accountChange();
            }
            catch (InvalidAccessTokenException ex)
            {
                _logger.LogWarning(ex, "A client tried to change an account with an access token that is not valid.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.SessionExpired);
            }
            catch (RequiredFieldMissingException ex)
            {
                _logger.LogInformation(ex, "An account change arrived with an empty field.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.RequiredField);
            }
            catch (DisallowedCharactersException ex)
            {
                _logger.LogWarning(ex, "An account change arrived with characters that are not allowed.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.DisallowedCharacters);
            }
            catch (InvalidCredentialsException ex)
            {
                _logger.LogWarning(ex, "An account change arrived with an incorrect password.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.InvalidCredentials);
            }
            catch (WeakPasswordException ex)
            {
                _logger.LogInformation(ex, "An account change arrived with a password that does not meet the policy.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.WeakPassword);
            }
            catch (SamePasswordException ex)
            {
                _logger.LogInformation(ex, "An account change arrived with the password the account already has.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.SamePassword);
            }
            catch (InvalidEmailFormatException ex)
            {
                _logger.LogInformation(ex, "An account change arrived with an email that is not a valid address.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.InvalidEmailFormat);
            }
            catch (EmailAlreadyRegisteredException ex)
            {
                _logger.LogInformation(ex, "An account change arrived with an email that already belongs to an account.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.EmailAlreadyRegistered);
            }
            catch (VerificationCodeRejectedException ex)
            {
                _logger.LogWarning(ex, "A code of an account change was rejected as {Reason}.", ex.Reason);
                throw ServiceFaultCommon.CreateRejectedCodeFault(ex.Reason);
            }
            catch (VerificationResendLimitException ex)
            {
                _logger.LogWarning(ex, "A new code of an account change was refused by the limit of codes.");
                throw ServiceFaultCommon.CreateResendLimitFault(ex);
            }
            catch (VerificationEmailNotSentException ex)
            {
                _logger.LogError(ex, "The code of an account change could not be emailed.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.CodeNotSent);
            }
        }
    }
}
