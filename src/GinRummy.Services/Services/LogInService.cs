using System;
using System.ServiceModel;

using GinRummy.Application.Ports;
using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Faults;
using GinRummy.Contracts.Services;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Services;
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
                throw CreateServiceFault(ServiceFaultCode.RequiredField);
            }
            catch (DisallowedCharactersException ex)
            {
                _logger.LogWarning(ex, "A sign-in arrived with characters that are not allowed.");
                throw CreateServiceFault(ServiceFaultCode.DisallowedCharacters);
            }
            catch (InvalidCredentialsException ex)
            {
                _logger.LogWarning(ex, "A sign-in with the email {Email} failed because of its credentials.", email);
                throw CreateServiceFault(ServiceFaultCode.InvalidCredentials);
            }
            catch (EmailNotVerifiedException ex)
            {
                _logger.LogInformation(ex, "An account with an unverified email tried to sign in.");
                throw CreateServiceFault(ServiceFaultCode.EmailNotVerified);
            }
            catch (AccountBannedException ex)
            {
                _logger.LogInformation(ex, "A banned account tried to sign in.");
                throw CreateSanctionFault(ex);
            }
            catch (VerificationEmailNotSentException ex)
            {
                _logger.LogError(ex, "The code of the second factor could not be emailed.");
                throw CreateServiceFault(ServiceFaultCode.CodeNotSent);
            }

            return outcome;
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
                throw CreateServiceFault(ServiceFaultCode.RequiredField);
            }
            catch (DisallowedCharactersException ex)
            {
                _logger.LogWarning(ex, "A second factor confirmation arrived with characters that are not allowed.");
                throw CreateServiceFault(ServiceFaultCode.DisallowedCharacters);
            }
            catch (VerificationCodeRejectedException ex)
            {
                _logger.LogWarning(ex, "A second factor code for the email {Email} was rejected as {Reason}.", email, ex.Reason);
                throw CreateServiceFault(MapRejectionReason(ex.Reason));
            }
            catch (AccountBannedException ex)
            {
                _logger.LogInformation(ex, "A banned account tried to confirm its sign-in.");
                throw CreateSanctionFault(ex);
            }

            return outcome;
        }

        private static FaultException<ServiceFault> CreateServiceFault(ServiceFaultCode code)
        {
            return new FaultException<ServiceFault>(new ServiceFault { Code = code });
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

        private static ServiceFaultCode MapRejectionReason(VerificationCodeCheckResult reason)
        {
            ServiceFaultCode code;
            switch (reason)
            {
                case VerificationCodeCheckResult.InvalidFormat:
                    code = ServiceFaultCode.CodeInvalidFormat;
                    break;
                case VerificationCodeCheckResult.AlreadyUsed:
                    code = ServiceFaultCode.CodeAlreadyUsed;
                    break;
                case VerificationCodeCheckResult.Expired:
                    code = ServiceFaultCode.CodeExpired;
                    break;
                case VerificationCodeCheckResult.TooManyAttempts:
                    code = ServiceFaultCode.CodeTooManyAttempts;
                    break;
                default:
                    code = ServiceFaultCode.CodeIncorrect;
                    break;
            }

            return code;
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
