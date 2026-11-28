using System;
using System.ServiceModel;
using System.Windows.Input;

using GinRummy.Client.LogInServiceReference;
using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.ViewModels
{
    public class LogInViewModel : ViewModelBase
    {
        private const string RequiredFieldKey = "Error_ValRequiredField";

        private readonly Func<ILogInService> _logInServiceFactory;
        private readonly ILogger<LogInViewModel> _logger;
        private string _password;
        private string _errorMessageKey;
        private bool _isResendCodeOffered;

        // A client is created for every attempt because a client whose channel failed cannot be opened again.
        public LogInViewModel(Func<ILogInService> logInServiceFactory, ILogger<LogInViewModel> logger)
        {
            _logInServiceFactory = logInServiceFactory ?? throw new ArgumentNullException(nameof(logInServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Email = string.Empty;
            _password = string.Empty;
            LogInCommand = new RelayCommand(LogIn);
        }

        public event EventHandler<LogInOutcome> SignedIn;
        public event EventHandler<LogInOutcome> GuestSignedIn;
        public event EventHandler<LogInCredentials> TwoFactorRequired;
        public event EventHandler<AccountRestriction> AccountRestricted;
        public event EventHandler<ErrorDialogKind> ConnectionFailed;

        public string Email { get; set; }
        public bool IsKeepSignedIn { get; set; }
        public ICommand LogInCommand { get; private set; }

        public int EmailMaxLength
        {
            get { return LogInCredentials.EmailMaxLength; }
        }

        public int PasswordMaxLength
        {
            get { return LogInCredentials.PasswordMaxLength; }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessageKey
        {
            get { return _errorMessageKey; }
            private set
            {
                _errorMessageKey = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasErrorMessage));
            }
        }

        public bool HasErrorMessage
        {
            get { return !string.IsNullOrEmpty(_errorMessageKey) ? true : false; }
        }

        public bool IsResendCodeOffered
        {
            get { return _isResendCodeOffered; }
            private set
            {
                _isResendCodeOffered = value;
                OnPropertyChanged();
            }
        }

        // A guest needs no credentials, so the main menu asks for its session directly (CU-02 FA-01).
        public void LogInAsGuest()
        {
            LogInOutcome outcome = RequestLogIn(logInService => logInService.LogInAsGuest());
            if (outcome != null)
            {
                GuestSignedIn?.Invoke(this, outcome);
            }
        }

        // The empty fields are checked here only to answer without waiting for the network (CU-02 FA-02).
        // The server repeats the check, together with every other rule of the sign-in.
        private void LogIn()
        {
            ErrorMessageKey = null;
            IsResendCodeOffered = false;
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessageKey = RequiredFieldKey;
                return;
            }

            LogInOutcome outcome = RequestLogIn(logInService => logInService.LogIn(Email, Password, IsKeepSignedIn));
            if (outcome == null)
            {
                return;
            }

            if (outcome.Status != LogInStatus.Accepted)
            {
                ShowRefusal(outcome);
            }
            else if (outcome.IsTwoFactorRequired)
            {
                TwoFactorRequired?.Invoke(this, new LogInCredentials(Email, Password, IsKeepSignedIn));
            }
            else
            {
                SignedIn?.Invoke(this, outcome);
            }
        }

        private void ShowRefusal(LogInOutcome outcome)
        {
            _logger.LogInformation("The server refused the sign-in as {Status}.", outcome.Status);
            if (outcome.Status == LogInStatus.AccountBanned)
            {
                AccountRestricted?.Invoke(this, LogInResultCommon.ToRestriction(outcome));
            }
            else
            {
                ErrorMessageKey = LogInResultCommon.ResolveMessageKey(outcome.Status);
                IsResendCodeOffered = outcome.Status == LogInStatus.EmailNotVerified ? true : false;
            }
        }

        private LogInOutcome RequestLogIn(Func<ILogInService, LogInOutcome> logInOperation)
        {
            LogInOutcome outcome = null;
            ILogInService logInService = _logInServiceFactory();
            ICommunicationObject communicationObject = (ICommunicationObject)logInService;
            try
            {
                outcome = logInOperation(logInService);
                communicationObject.Close();
            }
            catch (FaultException<ServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not complete the sign-in, with the fault {FaultCode}.", ex.Detail.Code);
                ErrorMessageKey = LogInResultCommon.ResolveFaultMessageKey(ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not complete the sign-in.");
                Password = string.Empty;
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
