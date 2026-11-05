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
    /// <summary>
    /// Implements the sign-up contract. Delegates each operation to its port and turns the broken rules into
    /// the faults the contract declares, so the client can choose the message of each one.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class SignUpService : ISignUpService
    {
        private readonly ISignUpPort _signUpPort;
        private readonly IEmailVerificationPort _emailVerificationPort;
        private readonly ILogger<SignUpService> _logger;

        /// <summary>
        /// Builds the service with the ports and the logger the composition root assembled.
        /// </summary>
        /// <param name="signUpPort">Port of the creation of an account.</param>
        /// <param name="emailVerificationPort">Port of the verification of the email and of its new codes.</param>
        /// <param name="logger">Logger of the server.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public SignUpService(
            ISignUpPort signUpPort,
            IEmailVerificationPort emailVerificationPort,
            ILogger<SignUpService> logger)
        {
            _signUpPort = signUpPort ?? throw new ArgumentNullException(nameof(signUpPort));
            _emailVerificationPort = emailVerificationPort ?? throw new ArgumentNullException(nameof(emailVerificationPort));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates an account and emails the code that verifies its email.
        /// </summary>
        /// <param name="newAccount">What the person typed, with the language the client was showing.</param>
        /// <returns>The stored email of the account and whether its code was sent.</returns>
        public SignUpOutcome SignUp(NewAccount newAccount)
        {
            SignUpOutcome outcome;
            try
            {
                SignUpForm form = SignUpFormMapper.MapToSignUpForm(newAccount);
                SignUpResult result = _signUpPort.SignUp(form, newAccount?.LocaleCode);
                LogCreatedAccount(result);
                outcome = SignUpOutcomeMapper.MapToSignUpOutcome(result);
            }
            catch (RequiredFieldMissingException ex)
            {
                _logger.LogWarning(ex, "A sign-up arrived with an empty field.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.RequiredField);
            }
            catch (DisallowedCharactersException ex)
            {
                _logger.LogWarning(
                    ex,
                    "A sign-up from {Caller} arrived with characters that are not allowed.",
                    CallerAddressCommon.DescribeCaller());
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.DisallowedCharacters);
            }
            catch (InvalidEmailFormatException ex)
            {
                _logger.LogInformation(ex, "A sign-up arrived with an email that is not a valid address.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.InvalidEmailFormat);
            }
            catch (FieldTooLongException ex)
            {
                _logger.LogWarning(ex, "A sign-up arrived with a username longer than its column.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.FieldTooLong);
            }
            catch (WeakPasswordException ex)
            {
                _logger.LogInformation(ex, "A sign-up arrived with a password that does not meet the policy.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.WeakPassword);
            }
            catch (EmailAlreadyRegisteredException ex)
            {
                _logger.LogInformation(ex, "A sign-up arrived with an email that already belongs to an account.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.EmailAlreadyRegistered);
            }

            return outcome;
        }

        /// <summary>
        /// Verifies the email of an account with the code emailed to it.
        /// </summary>
        /// <param name="email">Email of the account, as the server stored it.</param>
        /// <param name="verificationCode">Code that verifies the email, as the player typed it.</param>
        public void VerifyEmailByVerificationCode(string email, string verificationCode)
        {
            try
            {
                _emailVerificationPort.VerifyEmail(email, verificationCode);
                _logger.LogInformation("The email {Email} was verified.", email);
            }
            catch (RequiredFieldMissingException ex)
            {
                _logger.LogWarning(ex, "An email verification arrived with an empty field.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.RequiredField);
            }
            catch (DisallowedCharactersException ex)
            {
                _logger.LogWarning(ex, "An email verification arrived with characters that are not allowed.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.DisallowedCharacters);
            }
            catch (EmailAlreadyVerifiedException ex)
            {
                _logger.LogInformation(ex, "The email {Email} was verified again.", email);
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.EmailAlreadyVerified);
            }
            catch (VerificationCodeRejectedException ex)
            {
                _logger.LogWarning(ex, "A code that verifies the email {Email} was rejected as {Reason}.", email, ex.Reason);
                throw ServiceFaultCommon.CreateRejectedCodeFault(ex.Reason);
            }
        }

        /// <summary>
        /// Emails a new code to an account whose email is not verified yet.
        /// </summary>
        /// <param name="email">Email of the account, as the server stored it.</param>
        public void ResendVerificationCode(string email)
        {
            try
            {
                _emailVerificationPort.ResendVerificationCode(email);
                _logger.LogInformation("A new code that verifies the email {Email} was requested.", email);
            }
            catch (RequiredFieldMissingException ex)
            {
                _logger.LogWarning(ex, "A request for a new code arrived with an empty email.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.RequiredField);
            }
            catch (DisallowedCharactersException ex)
            {
                _logger.LogWarning(ex, "A request for a new code arrived with characters that are not allowed.");
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.DisallowedCharacters);
            }
            catch (EmailAlreadyVerifiedException ex)
            {
                _logger.LogInformation(ex, "A new code was requested for the email {Email}, which is already verified.", email);
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.EmailAlreadyVerified);
            }
            catch (VerificationResendLimitException ex)
            {
                _logger.LogWarning(ex, "A new code for the email {Email} was refused by the limit of codes.", email);
                throw ServiceFaultCommon.CreateResendLimitFault(ex);
            }
            catch (VerificationEmailNotSentException ex)
            {
                _logger.LogError(ex, "The new code that verifies the email {Email} could not be emailed.", email);
                throw ServiceFaultCommon.CreateServiceFault(ServiceFaultCode.CodeNotSent);
            }
        }

        private void LogCreatedAccount(SignUpResult result)
        {
            if (result.IsVerificationCodeSent)
            {
                _logger.LogInformation("Player {PlayerId} created an account and was emailed its code.", result.Account.PlayerId);
            }
            else
            {
                _logger.LogError("Player {PlayerId} created an account, but its code could not be emailed.", result.Account.PlayerId);
            }
        }
    }
}
