using System;
using System.Globalization;
using System.ServiceModel;
using System.Windows.Input;

using GinRummy.Client.Mvvm;
using GinRummy.Client.SignUpServiceReference;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.ViewModels
{
    public class SignUpViewModel : ViewModelBase
    {
        private const string RequiredFieldKey = "Error_ValRequiredField";
        private const string PasswordMismatchKey = "Error_ValPasswordMismatch";

        // The username column holds twenty characters, the same limit the field of the screen has.
        private const int UsernameMaxLength = 20;

        private readonly Func<ISignUpService> _signUpServiceFactory;
        private readonly ILogger<SignUpViewModel> _logger;
        private string _password;
        private string _confirmPassword;
        private string _errorMessageKey;
        private object _errorMessageArgument;

        // A client is created for every attempt because a client whose channel failed cannot be opened again.
        public SignUpViewModel(
            Func<ISignUpService> signUpServiceFactory,
            ILogger<SignUpViewModel> logger,
            bool isGuestSessionActive)
        {
            _signUpServiceFactory = signUpServiceFactory ?? throw new ArgumentNullException(nameof(signUpServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            IsGuestSessionNoticeShown = isGuestSessionActive;
            Email = string.Empty;
            Username = string.Empty;
            _password = string.Empty;
            _confirmPassword = string.Empty;
            CreateAccountCommand = new RelayCommand(CreateAccount);
        }

        public event EventHandler<SignUpOutcome> AccountCreated;
        public event EventHandler<ErrorDialogKind> ConnectionFailed;

        public string Email { get; set; }
        public string Username { get; set; }
        public bool IsGuestSessionNoticeShown { get; private set; }
        public ICommand CreateAccountCommand { get; private set; }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set
            {
                _confirmPassword = value;
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

        public object ErrorMessageArgument
        {
            get { return _errorMessageArgument; }
            private set
            {
                _errorMessageArgument = value;
                RaisePropertyChanged();
            }
        }

        public bool HasErrorMessage
        {
            get { return !string.IsNullOrEmpty(_errorMessageKey); }
        }

        // The empty fields are checked here only to answer without waiting for the network (CU-01 FA-01).
        // Matching the password with its confirmation is the only check the client resolves on its own (CU-01 FA-04).
        private void CreateAccount()
        {
            ErrorMessageArgument = null;
            ErrorMessageKey = null;
            if (IsAnyFieldEmpty())
            {
                ErrorMessageKey = RequiredFieldKey;
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessageKey = PasswordMismatchKey;
                return;
            }

            SignUpOutcome outcome = RequestSignUp();
            if (outcome != null)
            {
                AccountCreated?.Invoke(this, outcome);
            }
        }

        private bool IsAnyFieldEmpty()
        {
            return string.IsNullOrEmpty(Email)
                || string.IsNullOrEmpty(Username)
                || string.IsNullOrEmpty(Password)
                || string.IsNullOrEmpty(ConfirmPassword);
        }

        // The localization provider sets the interface culture of the thread, so it names the language the screen shows (CU-01 step 6).
        private NewAccount BuildNewAccount()
        {
            NewAccount newAccount = new NewAccount
            {
                Email = Email,
                Username = Username,
                Password = Password,
                LocaleCode = CultureInfo.CurrentUICulture.Name
            };

            return newAccount;
        }

        private SignUpOutcome RequestSignUp()
        {
            SignUpOutcome outcome = null;
            ISignUpService signUpService = _signUpServiceFactory();
            ICommunicationObject communicationObject = (ICommunicationObject)signUpService;
            try
            {
                outcome = signUpService.SignUp(BuildNewAccount());
                communicationObject.Close();
            }
            catch (FaultException<ServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogInformation(ex, "The server refused the sign-up with the fault {FaultCode}.", ex.Detail.Code);
                ShowServiceFault(ex.Detail.Code);
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not complete the sign-up.");
                ReportConnectionFailure(ErrorDialogKind.ServiceUnavailable);
            }
            catch (EndpointNotFoundException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The sign-up service could not be reached.");
                ReportConnectionFailure(ErrorDialogKind.ConnectionRefused);
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The connection with the sign-up service was lost.");
                ReportConnectionFailure(ErrorDialogKind.ConnectionLost);
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The sign-up service timed out.");
                ReportConnectionFailure(ErrorDialogKind.ConnectionLost);
            }

            return outcome;
        }

        private void ShowServiceFault(ServiceFaultCode code)
        {
            if (code == ServiceFaultCode.FieldTooLong)
            {
                ErrorMessageArgument = UsernameMaxLength;
            }

            ErrorMessageKey = SignUpFaultCommon.ResolveMessageKey(code);
        }

        private void ReportConnectionFailure(ErrorDialogKind kind)
        {
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            ConnectionFailed?.Invoke(this, kind);
        }
    }
}
