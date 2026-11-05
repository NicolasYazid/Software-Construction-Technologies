using System;
using System.ServiceModel;

using GinRummy.Client.Models;
using GinRummy.Client.SignUpServiceReference;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.ViewModels
{
    public class SignUpVerifyEmailViewModel : VerifyEmailViewModel
    {
        private const string RequiredFieldKey = "Error_ValRequiredField";
        private const string ResendLimitReachedKey = "Error_AuthResendLimitReached";
        private const int AttemptJustUsed = 1;
        private readonly Func<ISignUpService> _signUpServiceFactory;
        private readonly ILogger<SignUpVerifyEmailViewModel> _logger;

        // A client is created for every call because a client whose channel failed cannot be opened again.
        public SignUpVerifyEmailViewModel(
            string email,
            Func<ISignUpService> signUpServiceFactory,
            ILogger<SignUpVerifyEmailViewModel> logger)
            : base(VerificationPurpose.AccountSignUp, email)
        {
            _signUpServiceFactory = signUpServiceFactory ?? throw new ArgumentNullException(nameof(signUpServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // The account exists even when its first email failed, so the screen opens telling it (CU-01 EX-06).
        public void ShowCodeNotSent()
        {
            IsCodeNotSentShown = true;
        }

        protected override void Verify()
        {
            ErrorMessageKey = null;
            if (string.IsNullOrEmpty(Code))
            {
                ErrorMessageKey = RequiredFieldKey;
                return;
            }

            if (CallSignUpService(VerifyCode))
            {
                base.Verify();
            }
        }

        // A new code replaces the previous one on the server, so its time and its attempts start over (CU-09 FA-01).
        protected override void ResendCode()
        {
            ErrorMessageKey = null;
            IsCodeNotSentShown = false;
            if (CallSignUpService(RequestNewCode))
            {
                StartNewCode();
            }
        }

        private void VerifyCode(ISignUpService signUpService)
        {
            signUpService.VerifyEmailByVerificationCode(DestinationAddress, Code);
        }

        private void RequestNewCode(ISignUpService signUpService)
        {
            signUpService.ResendVerificationCode(DestinationAddress);
        }

        private void StartNewCode()
        {
            base.ResendCode();
            RemainingAttempts = MaximumAttempts;
        }

        // An email that was already verified is not an error, so the screen shows its notice instead of a message (CU-09 FA-02).
        private void ShowServiceFault(ServiceFaultCode code)
        {
            if (code == ServiceFaultCode.EmailAlreadyVerified)
            {
                StopCountdowns();
                IsAlreadyVerified = true;
            }
            else
            {
                ShowRejection(code);
            }
        }

        private void ShowRejection(ServiceFaultCode code)
        {
            switch (code)
            {
                case ServiceFaultCode.CodeIncorrect:
                    RemainingAttempts = Math.Max(RemainingAttempts - AttemptJustUsed, 0);
                    break;
                case ServiceFaultCode.CodeTooManyAttempts:
                    RemainingAttempts = 0;
                    break;
                case ServiceFaultCode.CodeNotSent:
                    // The new code exists even though its email failed, so its time and the wait for another one start anyway.
                    StartNewCode();
                    break;
                default:
                    break;
            }

            ErrorMessageKey = SignUpFaultCommon.ResolveMessageKey(code);
        }

        private bool CallSignUpService(Action<ISignUpService> operation)
        {
            bool isCompleted = false;
            ISignUpService signUpService = _signUpServiceFactory();
            ICommunicationObject communicationObject = (ICommunicationObject)signUpService;
            try
            {
                operation(signUpService);
                communicationObject.Close();
                isCompleted = true;
            }
            catch (FaultException<ResendLimitFault> ex)
            {
                communicationObject.Abort();
                _logger.LogInformation(ex, "The server refused a new code for {RetryAfterSeconds} seconds.", ex.Detail.RetryAfterSeconds);
                ErrorMessageKey = ResendLimitReachedKey;
                RestartResendCountdown(TimeSpan.FromSeconds(ex.Detail.RetryAfterSeconds));
            }
            catch (FaultException<ServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogInformation(ex, "The server refused the email verification with the fault {FaultCode}.", ex.Detail.Code);
                ShowServiceFault(ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not complete the email verification.");
                RequestErrorDialog(ErrorDialogKind.ServiceUnavailable);
            }
            catch (EndpointNotFoundException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The sign-up service could not be reached.");
                RequestErrorDialog(ErrorDialogKind.ConnectionRefused);
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The connection with the sign-up service was lost.");
                RequestErrorDialog(ErrorDialogKind.ConnectionLost);
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The sign-up service timed out.");
                RequestErrorDialog(ErrorDialogKind.ConnectionLost);
            }

            return isCompleted;
        }
    }
}
