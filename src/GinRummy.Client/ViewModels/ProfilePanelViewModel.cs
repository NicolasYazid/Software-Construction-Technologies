using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class ProfilePanelViewModel : ViewModelBase
    {
        private string _email;
        private string _newEmail;
        private string _pendingEmail;
        private bool _isEmailChangedShown;
        private bool _isChangeEmailOpen;
        private bool _isEmailRequiredShown;
        private bool _isTwoStepEnabled;
        private bool _isTwoStepDisabledShown;

        public ProfilePanelViewModel(SampleDataService dataService)
        {
            if (dataService == null)
            {
                throw new ArgumentNullException(nameof(dataService));
            }

            AccountSettings settings = dataService.GetAccountSettings();
            _email = settings.Email;
            _newEmail = string.Empty;
            _isTwoStepEnabled = settings.IsTwoStepEnabled;
            MasterVolume = settings.MasterVolume;
            IsChatFilterEnabled = settings.IsChatFilterEnabled;
            LinkedAccounts = new ObservableCollection<LinkedAccount>(settings.LinkedAccounts);
            ChangeEmailCommand = new RelayCommand(OpenChangeEmail);
            CancelEmailCommand = new RelayCommand(CancelChangeEmail);
            SaveEmailCommand = new RelayCommand(SaveEmail);
            ToggleTwoStepCommand = new RelayCommand(ToggleTwoStep);
            LinkCommand = new RelayCommand(LinkAccount);
            UnlinkCommand = new RelayCommand(UnlinkAccount);
        }

        public event EventHandler EmailVerificationRequested;

        public event EventHandler TwoStepEnableRequested;

        public event EventHandler TwoStepDisableRequested;

        public ObservableCollection<LinkedAccount> LinkedAccounts { get; private set; }
        public double MasterVolume { get; set; }
        public bool IsChatFilterEnabled { get; set; }
        public ICommand ChangeEmailCommand { get; private set; }
        public ICommand CancelEmailCommand { get; private set; }
        public ICommand SaveEmailCommand { get; private set; }
        public ICommand ToggleTwoStepCommand { get; private set; }
        public ICommand LinkCommand { get; private set; }
        public ICommand UnlinkCommand { get; private set; }

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

        public bool IsEmailRequiredShown
        {
            get { return _isEmailRequiredShown; }
            private set
            {
                _isEmailRequiredShown = value;
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

        public bool IsTwoStepDisabledShown
        {
            get { return _isTwoStepDisabledShown; }
            private set
            {
                _isTwoStepDisabledShown = value;
                RaisePropertyChanged();
            }
        }

        public void CompleteEmailChange()
        {
            Email = _pendingEmail;
            IsEmailChangedShown = true;
        }

        // Cancelling returns the switch to where it was (CU-05 FA-02).
        public void CompleteTwoStepDisable(bool isConfirmed)
        {
            if (isConfirmed)
            {
                IsTwoStepDisabledShown = true;
            }
            else
            {
                IsTwoStepEnabled = true;
            }
        }

        private void OpenChangeEmail()
        {
            NewEmail = string.Empty;
            IsEmailRequiredShown = false;
            IsChangeEmailOpen = true;
        }

        private void CancelChangeEmail()
        {
            IsChangeEmailOpen = false;
        }

        // Only the empty field is caught here (CU-07 FA-02).
        // The server checks the format and the uniqueness of the address before it sends the code.
        private void SaveEmail()
        {
            string newEmail = (_newEmail ?? string.Empty).Trim();
            IsEmailRequiredShown = newEmail.Length == 0;
            if (!IsEmailRequiredShown)
            {
                IsChangeEmailOpen = false;
                _pendingEmail = newEmail;
                EmailVerificationRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        // Enabling the second factor requires a confirmed code (CU-04), and disabling it requires a confirmation (CU-05).
        private void ToggleTwoStep()
        {
            IsTwoStepDisabledShown = false;
            if (_isTwoStepEnabled)
            {
                TwoStepEnableRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                TwoStepDisableRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void LinkAccount(object parameter)
        {
            ReplaceLinkedAccount(parameter as LinkedAccount, true);
        }

        private void UnlinkAccount(object parameter)
        {
            ReplaceLinkedAccount(parameter as LinkedAccount, false);
        }

        // A linked account does not notify its changes, so the list only sees a new state when the row is replaced.
        private void ReplaceLinkedAccount(LinkedAccount account, bool isLinked)
        {
            int index = LinkedAccounts.IndexOf(account);
            if (index >= 0)
            {
                LinkedAccount updated = new LinkedAccount();
                updated.PlatformName = account.PlatformName;
                updated.IsLinked = isLinked;
                LinkedAccounts[index] = updated;
            }
        }
    }
}
