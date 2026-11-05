using System;
using System.ServiceModel;
using System.Windows.Input;

using GinRummy.Client.AccountSettingsServiceReference;
using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using Microsoft.Extensions.Logging;

using IPlayerProfileService = GinRummy.Client.PlayerProfileServiceReference.IPlayerProfileService;
using ProfileServiceFault = GinRummy.Client.PlayerProfileServiceReference.ServiceFault;
using ProfileServiceFaultCode = GinRummy.Client.PlayerProfileServiceReference.ServiceFaultCode;

namespace GinRummy.Client.ViewModels
{
    public class ProfilePanelViewModel : ViewModelBase
    {
        private const string RequiredFieldKey = "Error_ValRequiredField";
        private const string SameEmailKey = "ProfilePanel_SameEmail";
        private const string TwoStepEnabledKey = "ProfilePanel_TwoStepEnabled";
        private const string TwoStepDisabledKey = "ProfilePanel_TwoStepDisabled";

        private readonly Func<IPlayerProfileService> _playerProfileServiceFactory;
        private readonly AccountSettingsCaller _accountSettingsCaller;
        private readonly ILogger<ProfilePanelViewModel> _logger;
        private string _email;
        private string _newEmail;
        private string _pendingEmail;
        private string _emailErrorKey;
        private bool _isEmailChangedShown;
        private bool _isChangeEmailOpen;
        private bool _isPasswordChangedShown;
        private bool _isTwoStepEnabled;
        private string _twoStepNoticeKey;
        private string _twoStepErrorKey;

        // A client is created for every call because a client whose channel failed cannot be opened again.
        public ProfilePanelViewModel(
            Func<IPlayerProfileService> playerProfileServiceFactory,
            AccountSettingsCaller accountSettingsCaller,
            ILogger<ProfilePanelViewModel> logger)
        {
            _playerProfileServiceFactory = playerProfileServiceFactory
                ?? throw new ArgumentNullException(nameof(playerProfileServiceFactory));
            _accountSettingsCaller = accountSettingsCaller ?? throw new ArgumentNullException(nameof(accountSettingsCaller));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _email = string.Empty;
            _newEmail = string.Empty;
            IsChatFilterEnabled = true;
            ChangeEmailCommand = new RelayCommand(OpenChangeEmail);
            CancelEmailCommand = new RelayCommand(CancelChangeEmail);
            SaveEmailCommand = new RelayCommand(SaveEmail);
            ToggleTwoStepCommand = new RelayCommand(ToggleTwoStep);
        }

        public event EventHandler EmailVerificationRequested;

        public event EventHandler TwoStepEnableRequested;

        public event EventHandler TwoStepDisableRequested;

        public event EventHandler<ErrorDialogKind> ErrorDialogRequested;

        public ICommand ChangeEmailCommand { get; private set; }
        public ICommand CancelEmailCommand { get; private set; }
        public ICommand SaveEmailCommand { get; private set; }
        public ICommand ToggleTwoStepCommand { get; private set; }
        public bool IsChatFilterEnabled { get; set; }

        // Without a session there is no account, so the panel only offers the language (CU-20 FA-01).
        public bool IsSignedIn
        {
            get { return _accountSettingsCaller.HasSession; }
        }

        public string Email
        {
            get { return _email; }
            private set
            {
                _email = value;
                RaisePropertyChanged();
            }
        }

        public string NewEmail
        {
            get { return _newEmail; }
            set
            {
                _newEmail = value;
                RaisePropertyChanged();
            }
        }

        public string PendingEmail
        {
            get { return _pendingEmail; }
        }

        public string EmailErrorKey
        {
            get { return _emailErrorKey; }
            private set
            {
                _emailErrorKey = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasEmailError));
            }
        }

        public bool HasEmailError
        {
            get { return !string.IsNullOrEmpty(_emailErrorKey); }
        }

        public bool IsEmailChangedShown
        {
            get { return _isEmailChangedShown; }
            private set
            {
                _isEmailChangedShown = value;
                RaisePropertyChanged();
            }
        }

        public bool IsChangeEmailOpen
        {
            get { return _isChangeEmailOpen; }
            private set
            {
                _isChangeEmailOpen = value;
                RaisePropertyChanged();
            }
        }

        public bool IsPasswordChangedShown
        {
            get { return _isPasswordChangedShown; }
            private set
            {
                _isPasswordChangedShown = value;
                RaisePropertyChanged();
            }
        }

        public bool IsTwoStepEnabled
        {
            get { return _isTwoStepEnabled; }
            set
            {
                _isTwoStepEnabled = value;
                RaisePropertyChanged();
            }
        }

        public string TwoStepNoticeKey
        {
            get { return _twoStepNoticeKey; }
            private set
            {
                _twoStepNoticeKey = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasTwoStepNotice));
            }
        }

        public bool HasTwoStepNotice
        {
            get { return !string.IsNullOrEmpty(_twoStepNoticeKey); }
        }

        public string TwoStepErrorKey
        {
            get { return _twoStepErrorKey; }
            private set
            {
                _twoStepErrorKey = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasTwoStepError));
            }
        }

        public bool HasTwoStepError
        {
            get { return !string.IsNullOrEmpty(_twoStepErrorKey); }
        }

        // The settings are read from the server every time, because only the server knows what the codes already confirmed.
        public void LoadAccountSettings()
        {
            AccountSettingsDetails settings = null;
            if (IsSignedIn && CallAccountSettings((service, accessToken) => settings = service.GetAccountSettings(accessToken)))
            {
                Email = settings.Email;
                IsTwoStepEnabled = settings.IsTwoFactorEnabled;
            }
        }

        public void CompleteEmailChange()
        {
            LoadAccountSettings();
            IsEmailChangedShown = true;
        }

        public void CompletePasswordChange()
        {
            IsPasswordChangedShown = true;
        }

        public void CompleteTwoStepEnable()
        {
            LoadAccountSettings();
            if (_isTwoStepEnabled)
            {
                TwoStepNoticeKey = TwoStepEnabledKey;
            }
        }

        // The toggle stays on until the server turns the second factor off with the password of the player (CU-05).
        public void CompleteTwoStepDisable(bool isConfirmed, string password)
        {
            IsTwoStepEnabled = true;
            if (isConfirmed)
            {
                DisableTwoStep(password);
            }
        }

        // The language is already shown when this runs, so a failure only means it is not kept for the next session (CU-20 EX-05).
        public void SaveLanguage(string accessToken, string localeCode)
        {
            IPlayerProfileService profileService = _playerProfileServiceFactory();
            ICommunicationObject communicationObject = (ICommunicationObject)profileService;
            try
            {
                profileService.ChangeLanguage(accessToken, localeCode);
                communicationObject.Close();
            }
            catch (FaultException<ProfileServiceFault> ex)
            {
                communicationObject.Abort();
                _logger.LogInformation(
                    ex,
                    "The server refused to keep the language {LocaleCode} with the fault {FaultCode}.",
                    localeCode,
                    ex.Detail.Code);
                ErrorDialogKind kind = ErrorDialogKind.ServiceUnavailable;
                if (ex.Detail.Code == ProfileServiceFaultCode.SessionExpired)
                {
                    kind = ErrorDialogKind.SessionExpired;
                }

                RequestErrorDialog(kind);
            }
            catch (FaultException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The server could not keep the chosen language.");
                RequestErrorDialog(ErrorDialogKind.ServiceUnavailable);
            }
            catch (CommunicationException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The profile service could not be reached to keep the chosen language.");
                RequestErrorDialog(ErrorDialogKind.ConnectionLost);
            }
            catch (TimeoutException ex)
            {
                communicationObject.Abort();
                _logger.LogError(ex, "The profile service timed out while keeping the chosen language.");
                RequestErrorDialog(ErrorDialogKind.ConnectionLost);
            }
        }

        private void OpenChangeEmail()
        {
            NewEmail = string.Empty;
            EmailErrorKey = null;
            IsChangeEmailOpen = true;
        }

        private void CancelChangeEmail()
        {
            IsChangeEmailOpen = false;
        }

        // The empty field and the address the account already has are caught here, because neither needs the server (CU-07).
        // The server checks the format and the uniqueness of the address before it sends the code.
        private void SaveEmail()
        {
            IsEmailChangedShown = false;
            EmailErrorKey = null;
            string newEmail = (_newEmail ?? string.Empty).Trim();
            if (newEmail.Length == 0)
            {
                EmailErrorKey = RequiredFieldKey;
            }
            else if (string.Equals(newEmail, _email, StringComparison.OrdinalIgnoreCase))
            {
                EmailErrorKey = SameEmailKey;
            }
            else
            {
                RequestEmailChange(newEmail);
            }
        }

        private void RequestEmailChange(string newEmail)
        {
            if (CallAccountSettings((service, accessToken) => service.RequestEmailChange(accessToken, newEmail)))
            {
                IsChangeEmailOpen = false;
                _pendingEmail = newEmail;
                EmailVerificationRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                EmailErrorKey = _accountSettingsCaller.ErrorMessageKey;
            }
        }

        private void ToggleTwoStep()
        {
            TwoStepNoticeKey = null;
            TwoStepErrorKey = null;
            if (_isTwoStepEnabled)
            {
                RequestTwoStepActivation();
            }
            else
            {
                TwoStepDisableRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        // The toggle stays off until the code is confirmed, because only the server turns the second factor on (CU-04).
        private void RequestTwoStepActivation()
        {
            IsTwoStepEnabled = false;
            if (CallAccountSettings((service, accessToken) => service.RequestTwoFactorActivation(accessToken)))
            {
                TwoStepEnableRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                TwoStepErrorKey = _accountSettingsCaller.ErrorMessageKey;
            }
        }

        private void DisableTwoStep(string password)
        {
            if (CallAccountSettings((service, accessToken) => service.DisableTwoFactor(accessToken, password)))
            {
                IsTwoStepEnabled = false;
                TwoStepNoticeKey = TwoStepDisabledKey;
            }
            else
            {
                TwoStepErrorKey = _accountSettingsCaller.ErrorMessageKey;
            }
        }

        private bool CallAccountSettings(Action<IAccountSettingsService, string> operation)
        {
            bool isCompleted = _accountSettingsCaller.Call(operation);
            if (_accountSettingsCaller.ErrorDialog.HasValue)
            {
                RequestErrorDialog(_accountSettingsCaller.ErrorDialog.Value);
            }

            return isCompleted;
        }

        private void RequestErrorDialog(ErrorDialogKind kind)
        {
            ErrorDialogRequested?.Invoke(this, kind);
        }
    }
}
