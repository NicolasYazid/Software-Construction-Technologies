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
        public event EventHandler<LogInCredentials> TwoFactorRequired;
        public event EventHandler<AccountRestriction> AccountRestricted;
        public event EventHandler<ErrorDialogKind> ConnectionFailed;

        public string Email { get; set; }
        public bool IsKeepSignedIn { get; set; }
        public ICommand LogInCommand { get; private set; }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged();
            }
        }

        public string ErrorMessageKey
        {
            get { return _errorMessageKey; }
            private set
            {
                _errorMessageKey = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasErrorMessage));
            }
        }

        public bool HasErrorMessage
        {
            get { return !string.IsNullOrEmpty(_errorMessageKey); }
        }

        // The empty fields are checked here only to answer without waiting for the network (CU-02 FA-02).
        // The server repeats the check, together with every other rule of the sign-in.
        private void LogIn()
        {
            ErrorMessageKey = null;
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessageKey = RequiredFieldKey;
                return;
            }

            LogInOutcome outcome = RequestLogIn();
            if (outcome == null)
            {
                return;
            }

            if (outcome.IsTwoFactorRequired)
            {
                TwoFactorRequired?.Invoke(this, new LogInCredentials(Email, Password, IsKeepSignedIn));
            }
            else
            {
                SignedIn?.Invoke(this, outcome);
            }
        }

        private LogInOutcome RequestLogIn()
        {
            LogInOutcome outcome = null;
            ILogInService logInService = _logInServiceFactory();
            ICommunicationObject communicationObject = (ICommunicationObject)logInService;
            try
            {
                outcome = logInService.LogIn(Email, Password, IsKeepSignedIn);
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
                _logger.LogInformation(ex, "The server refused the sign-in with the fault {FaultCode}.", ex.Detail.Code);
                ErrorMessageKey = LogInFaultCommon.ResolveMessageKey(ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                // The server could not complete the sign-in, so the email is kept and the password is cleared (CU-02 EX-02).
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
