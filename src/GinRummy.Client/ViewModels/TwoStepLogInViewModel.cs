using System;
using System.ServiceModel;

using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.ViewModels
{
    public class TwoStepLogInViewModel : TwoStepViewModel
    {
        private const string RequiredFieldKey = "Error_ValRequiredField";
        private readonly LogInCredentials _credentials;
        private readonly Func<ILogInService> _logInServiceFactory;
        private readonly ILogger<TwoStepLogInViewModel> _logger;

        // A client is created for every call because a client whose channel failed cannot be opened again.
        public TwoStepLogInViewModel(
            LogInCredentials credentials,
            Func<ILogInService> logInServiceFactory,
            ILogger<TwoStepLogInViewModel> logger)
            : base(TwoStepPurpose.LogIn)
        {
            _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            _logInServiceFactory = logInServiceFactory ?? throw new ArgumentNullException(nameof(logInServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public event EventHandler<LogInOutcome> SignedIn;
        public event EventHandler<AccountRestriction> AccountRestricted;
        public event EventHandler<ErrorDialogKind> ConnectionFailed;

        protected override void Verify()
        {
            ErrorMessageKey = null;
            if (string.IsNullOrEmpty(Code))
            {
                ErrorMessageKey = RequiredFieldKey;
                return;
            }

            LogInOutcome outcome = CallLogInService(VerifyCode);
            if (outcome != null)
            {
                CompleteLogIn(outcome);
            }
        }

        // The server only issues a sign-in code after it checks the password, so a new code is asked for by signing in again.
        // Signing in again invalidates the previous code and restarts its ten minutes, as a resend does in CU-04 FA-03.
        protected override void ResendCode()
        {
            ErrorMessageKey = null;
            LogInOutcome outcome = CallLogInService(SignInAgain);
            if (outcome == null)
            {
                return;
            }

            if (outcome.IsTwoFactorRequired)
            {
                base.ResendCode();
            }
            else
            {
                CompleteLogIn(outcome);
            }
        }

        private LogInOutcome VerifyCode(ILogInService logInService)
        {
            return logInService.VerifyLogInByTwoFactorAuthentication(_credentials.Email, Code, _credentials.IsKeepSignedIn);
        }

        private LogInOutcome SignInAgain(ILogInService logInService)
        {
            return logInService.LogIn(_credentials.Email, _credentials.Password, _credentials.IsKeepSignedIn);
        }

        private void CompleteLogIn(LogInOutcome outcome)
        {
            CodeCountdown.Stop();
            SignedIn?.Invoke(this, outcome);
        }

        private LogInOutcome CallLogInService(Func<ILogInService, LogInOutcome> operation)
        {
            LogInOutcome outcome = null;
            ILogInService logInService = _logInServiceFactory();
            ICommunicationObject communicationObject = (ICommunicationObject)logInService;
            try
            {
                outcome = operation(logInService);
                communicationObject.Close();
            }
            catch (FaultException<SanctionFault> ex)
            {
                communicationObject.Abort();
                _logger.LogInformation(ex, "The sign-in was refused because the account is banned.");
                AccountRestricted?.Invoke(this, LogInFaultCommon.ToRestriction(ex.Detail));
            }
            catch (FaultException<ServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogInformation(ex, "The server refused the second factor with the fault {FaultCode}.", ex.Detail.Code);
                ErrorMessageKey = LogInFaultCommon.ResolveMessageKey(ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not complete the second factor.");
                ConnectionFailed?.Invoke(this, ErrorDialogKind.ServiceUnavailable);
            }
            catch (EndpointNotFoundException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The sign-in service could not be reached.");
                ConnectionFailed?.Invoke(this, ErrorDialogKind.ConnectionRefused);
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The connection with the sign-in service was lost.");
                ConnectionFailed?.Invoke(this, ErrorDialogKind.ConnectionLost);
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The sign-in service timed out.");
                ConnectionFailed?.Invoke(this, ErrorDialogKind.ConnectionLost);
            }

            return outcome;
        }
    }
}
