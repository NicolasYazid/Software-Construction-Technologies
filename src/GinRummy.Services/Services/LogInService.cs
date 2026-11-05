using System;
using System.ServiceModel;

using GinRummy.Application.Ports;
using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Faults;
using GinRummy.Contracts.Services;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Services.Mappers;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class LogInService : ILogInService
    {
        private readonly ILogInPort _logInPort;
        private readonly ITwoFactorLogInPort _twoFactorLogInPort;
        private readonly ILogger<LogInService> _logger;

        public LogInService(ILogInPort logInPort, ITwoFactorLogInPort twoFactorLogInPort, ILogger<LogInService> logger)
        {
            _logInPort = logInPort ?? throw new ArgumentNullException(nameof(logInPort));
            _twoFactorLogInPort = twoFactorLogInPort ?? throw new ArgumentNullException(nameof(twoFactorLogInPort));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public LogInOutcome LogIn(string email, string password, bool keepSignedIn)
        {
            LogInOutcome outcome;
            try
            {
                LogInResult result = _logInPort.LogIn(email, password, keepSignedIn);
                LogAcceptedLogIn(result);
                outcome = LogInOutcomeMapper.MapToLogInOutcome(result);
            }
            catch (RequiredFieldMissingException ex)
            {
                _logger.LogWarning(ex, "A sign-in arrived with an empty field.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.RequiredField);
            }
            catch (DisallowedCharactersException ex)
            {
                // CU-02 EX-07 asks for the attempt to be recorded with where it came from.
                _logger.LogWarning(
                    ex,
                    "A sign-in from {Caller} arrived with characters that are not allowed.",
                    CallerAddressCommon.DescribeCaller());
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.DisallowedCharacters);
            }
            catch (InvalidCredentialsException ex)
            {
                _logger.LogWarning(ex, "A sign-in with the email {Email} failed because of its credentials.", email);
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.InvalidCredentials);
            }
            catch (EmailNotVerifiedException ex)
            {
                _logger.LogInformation(ex, "An account with an unverified email tried to sign in.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.EmailNotVerified);
            }
            catch (AccountBannedException ex)
            {
                _logger.LogInformation(ex, "A banned account tried to sign in.");
                throw CreateSanctionFault(ex);
            }
            catch (VerificationResendLimitException ex)
            {
                _logger.LogWarning(ex, "A second factor code for the email {Email} was refused by the limit of codes.", email);
                throw ServiceFaultCommon.CreateResendLimitFault(ex);
            }
            catch (VerificationEmailNotSentException ex)
            {
                _logger.LogError(ex, "The code of the second factor could not be emailed.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.CodeNotSent);
            }

            return outcome;
        }

        public LogInOutcome LogInAsGuest()
        {
            GuestAccess guestAccess = _logInPort.LogInAsGuest();
            _logger.LogInformation("Guest {GuestSessionId} entered the lobby.", guestAccess.Guest.GuestSessionId);

            return LogInOutcomeMapper.MapToGuestOutcome(guestAccess);
        }

        public LogInOutcome VerifyLogInByTwoFactorAuthentication(string email, string verificationCode, bool keepSignedIn)
        {
            LogInOutcome outcome;
            try
            {
                LogInResult result = _twoFactorLogInPort.VerifyTwoFactorLogIn(email, verificationCode, keepSignedIn);
                LogAcceptedLogIn(result);
                outcome = LogInOutcomeMapper.MapToLogInOutcome(result);
            }
            catch (RequiredFieldMissingException ex)
            {
                _logger.LogWarning(ex, "A second factor confirmation arrived with an empty field.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.RequiredField);
            }
            catch (DisallowedCharactersException ex)
            {
                _logger.LogWarning(ex, "A second factor confirmation arrived with characters that are not allowed.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.DisallowedCharacters);
            }
            catch (VerificationCodeRejectedException ex)
            {
                _logger.LogWarning(ex, "A second factor code for the email {Email} was rejected as {Reason}.", email, ex.Reason);
                throw ServiceFaultCommon.CreateRejectedCodeFault(ex.Reason);
            }
            catch (AccountBannedException ex)
            {
                _logger.LogInformation(ex, "A banned account tried to confirm its sign-in.");
                throw CreateSanctionFault(ex);
            }

            return outcome;
        }

        private static FaultException<SanctionFault> CreateSanctionFault(AccountBannedException exception)
        {
            SanctionFault sanction = new SanctionFault
            {
                ReasonName = exception.ReasonName,
                ExpiresAt = exception.ExpiresAt
            };

            return new FaultException<SanctionFault>(sanction);
        }

        private void LogAcceptedLogIn(LogInResult result)
        {
            if (result.IsTwoFactorRequired)
            {
                _logger.LogInformation("Player {PlayerId} was emailed the code of the second factor.", result.Account.PlayerId);
            }
            else
            {
                _logger.LogInformation(
                    "Player {PlayerId} signed in with a token that expires at {ExpiresAt}.",
                    result.Account.PlayerId,
                    result.AccessToken.ExpiresAt);
            }
        }
    }
}
