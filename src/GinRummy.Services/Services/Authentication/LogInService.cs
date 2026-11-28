using System;
using System.ServiceModel;

using GinRummy.Application.Ports.Authentication;
using GinRummy.Contracts.DataContracts.Authentication;
using GinRummy.Contracts.Faults.Shared;
using GinRummy.Contracts.Services.Authentication;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Services.Mappers.Authentication;
using GinRummy.Services.Services.Shared;
using Microsoft.Extensions.Logging;
using DomainLogInStatus = GinRummy.Domain.Entities.Authentication.LogInStatus;

namespace GinRummy.Services.Services.Authentication
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

        public LogInOutcome LogIn(string email, string password, bool isKeepSignedIn)
        {
            LogInOutcome outcome;
            try
            {
                LogInResult result = _logInPort.LogIn(email, password, isKeepSignedIn);
                LogLogInResult(result, email);
                outcome = LogInOutcomeMapper.MapToLogInOutcome(result);
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

        public LogInOutcome VerifyLogInByTwoFactorAuthentication(string email, string verificationCode, bool isKeepSignedIn)
        {
            LogInResult result = _twoFactorLogInPort.VerifyTwoFactorLogIn(email, verificationCode, isKeepSignedIn);
            LogLogInResult(result, email);

            return LogInOutcomeMapper.MapToLogInOutcome(result);
        }

        private void LogLogInResult(LogInResult result, string email)
        {
            switch (result.Status)
            {
                case DomainLogInStatus.Accepted:
                    LogAcceptedLogIn(result);
                    break;
                case DomainLogInStatus.DisallowedCharacters:
                    _logger.LogWarning(
                        "A sign-in from {Caller} arrived with characters that are not allowed.",
                        CallerAddressCommon.DescribeCaller());
                    break;
                case DomainLogInStatus.RequiredField:
                case DomainLogInStatus.EmailNotVerified:
                case DomainLogInStatus.AccountBanned:
                    _logger.LogInformation("A sign-in was refused as {Status}.", result.Status);
                    break;
                default:
                    _logger.LogWarning("A sign-in with the email {Email} was refused as {Status}.", email, result.Status);
                    break;
            }
        }

        private void LogAcceptedLogIn(LogInResult result)
        {
            if (result.IsTwoFactorRequired)
            {
                _logger.LogInformation(
                    "Player {PlayerAccountId} was emailed the code of the second factor.",
                    result.Account.PlayerAccountId);
            }
            else
            {
                _logger.LogInformation(
                    "Player {PlayerAccountId} signed in with a token that expires at {ExpiresAt}.",
                    result.Account.PlayerAccountId,
                    result.AccessToken.ExpiresAt);
            }
        }
    }
}
