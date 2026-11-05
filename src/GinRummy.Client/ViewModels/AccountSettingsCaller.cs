using System;
using System.Collections.Generic;
using System.ServiceModel;

using GinRummy.Client.AccountSettingsServiceReference;
using GinRummy.Client.Models;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.ViewModels
{
    // The panel and the screens of the codes call the same service, so they share how its faults become messages and dialogs.
    // Every call of the settings belongs to the session of the player, so the token is kept here once.
    public class AccountSettingsCaller
    {
        private const string ResendLimitReachedKey = "Error_AuthResendLimitReached";
        private const string ServiceUnavailableKey = "Error_SysServiceUnavailable";

        private static readonly Dictionary<ServiceFaultCode, string> _messageKeys = new Dictionary<ServiceFaultCode, string>
        {
            { ServiceFaultCode.RequiredField, "Error_ValRequiredField" },
            { ServiceFaultCode.DisallowedCharacters, "Error_ValDisallowedCharacters" },
            { ServiceFaultCode.InvalidCredentials, "Error_AuthInvalidCurrentPassword" },
            { ServiceFaultCode.WeakPassword, "Error_ValWeakPassword" },
            { ServiceFaultCode.SamePassword, "Error_AuthSamePassword" },
            { ServiceFaultCode.InvalidEmailFormat, "Error_ValInvalidEmailFormat" },
            { ServiceFaultCode.EmailAlreadyRegistered, "Error_AuthEmailAlreadyRegistered" },
            { ServiceFaultCode.CodeNotSent, "Shared_CodeNotSent" },
            { ServiceFaultCode.CodeInvalidFormat, "Error_ValInvalidCodeFormat" },
            { ServiceFaultCode.CodeIncorrect, "Error_AuthCodeInvalid" },
            { ServiceFaultCode.CodeExpired, "Error_AuthCodeExpired" },
            { ServiceFaultCode.CodeAlreadyUsed, "Error_AuthCodeAlreadyUsed" },
            { ServiceFaultCode.CodeTooManyAttempts, "Error_AuthTooManyAttempts" }
        };

        private readonly string _accessToken;
        private readonly Func<IAccountSettingsService> _accountSettingsServiceFactory;
        private readonly ILogger<AccountSettingsCaller> _logger;

        // A client is created for every call because a client whose channel failed cannot be opened again.
        public AccountSettingsCaller(
            string accessToken,
            Func<IAccountSettingsService> accountSettingsServiceFactory,
            ILogger<AccountSettingsCaller> logger)
        {
            _accessToken = accessToken;
            _accountSettingsServiceFactory = accountSettingsServiceFactory
                ?? throw new ArgumentNullException(nameof(accountSettingsServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool HasSession
        {
            get { return !string.IsNullOrEmpty(_accessToken); }
        }

        public ServiceFaultCode? FaultCode { get; private set; }
        public string ErrorMessageKey { get; private set; }
        public TimeSpan ResendWait { get; private set; }
        public ErrorDialogKind? ErrorDialog { get; private set; }

        public bool Call(Action<IAccountSettingsService, string> operation)
        {
            FaultCode = null;
            ErrorMessageKey = null;
            ResendWait = TimeSpan.Zero;
            ErrorDialog = null;
            bool isCompleted = false;
            IAccountSettingsService accountSettingsService = _accountSettingsServiceFactory();
            ICommunicationObject communicationObject = (ICommunicationObject)accountSettingsService;
            try
            {
                operation(accountSettingsService, _accessToken);
                communicationObject.Close();
                isCompleted = true;
            }
            catch (FaultException<ResendLimitFault> ex)
            {
                communicationObject.Abort();
                _logger.LogInformation(ex, "The server refused a new code for {RetryAfterSeconds} seconds.", ex.Detail.RetryAfterSeconds);
                ErrorMessageKey = ResendLimitReachedKey;
                ResendWait = TimeSpan.FromSeconds(ex.Detail.RetryAfterSeconds);
            }
            catch (FaultException<ServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogInformation(ex, "The server refused an account change with the fault {FaultCode}.", ex.Detail.Code);
                ShowRefusal(ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not complete an account change.");
                ErrorDialog = ErrorDialogKind.ServiceUnavailable;
            }
            catch (EndpointNotFoundException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The account settings service could not be reached.");
                ErrorDialog = ErrorDialogKind.ConnectionRefused;
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The connection with the account settings service was lost.");
                ErrorDialog = ErrorDialogKind.ConnectionLost;
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The account settings service timed out.");
                ErrorDialog = ErrorDialogKind.ConnectionLost;
            }

            return isCompleted;
        }

        // An expired session cannot be fixed on the screen, so it asks for the dialog that leaves the lobby instead of a message.
        private void ShowRefusal(ServiceFaultCode code)
        {
            FaultCode = code;
            if (code == ServiceFaultCode.SessionExpired)
            {
                ErrorDialog = ErrorDialogKind.SessionExpired;
            }
            else
            {
                string messageKey;
                ErrorMessageKey = _messageKeys.TryGetValue(code, out messageKey) ? messageKey : ServiceUnavailableKey;
            }
        }
    }
}
