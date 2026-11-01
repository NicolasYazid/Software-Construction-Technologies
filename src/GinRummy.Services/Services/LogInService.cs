using System;
using System.ServiceModel;

using GinRummy.Application.Ports;
using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Faults;
using GinRummy.Contracts.Services;
using GinRummy.Domain;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Services;
using GinRummy.Services.Mappers;
using Microsoft.Extensions.Logging;

namespace GinRummy.Services.Services
{
    /// <summary>
    /// Implements the sign-in contract. Delegates each operation to its port and turns the broken rules
    /// into the faults the contract declares, so the client can choose the message of each one.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class LogInService : ILogInService
    {
        private readonly ILogInPort _logInPort;
        private readonly ITwoFactorLogInPort _twoFactorLogInPort;
        private readonly ILogger<LogInService> _logger;

        /// <summary>
        /// Builds the service with the ports and the logger the composition root assembled.
        /// </summary>
        /// <param name="logInPort">Port of the sign-in with email and password.</param>
        /// <param name="twoFactorLogInPort">Port of the confirmation with the second factor.</param>
        /// <param name="logger">Logger of the server.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public LogInService(ILogInPort logInPort, ITwoFactorLogInPort twoFactorLogInPort, ILogger<LogInService> logger)
        {
            _logInPort = logInPort ?? throw new ArgumentNullException(nameof(logInPort));
            _twoFactorLogInPort = twoFactorLogInPort ?? throw new ArgumentNullException(nameof(twoFactorLogInPort));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Signs a player in with the email and the password of the account.
        /// </summary>
        /// <param name="email">Email of the account, as the player typed it.</param>
        /// <param name="password">Password of the account, as the player typed it.</param>
        /// <param name="keepSignedIn">Whether the player asked to stay signed in.</param>
        /// <returns>The opened session, or the notice that the code of the second factor is pending.</returns>
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
                // CU-02 EX-01 asks for every failed attempt to be recorded.
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

        /// <summary>
        /// Completes a sign-in whose password was accepted, with the code emailed to the account.
        /// </summary>
        /// <param name="email">Email of the account, as the player typed it to sign in.</param>
        /// <param name="verificationCode">Code of the second factor, as the player typed it.</param>
        /// <param name="keepSignedIn">Whether the player asked to stay signed in.</param>
        /// <returns>The opened session.</returns>
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
                    "Player {PlayerId} signed in with session {PlayerSessionId}.",
                    result.Account.PlayerId,
                    result.OpenedSession.PlayerSessionId);
            }
        }
    }
}
