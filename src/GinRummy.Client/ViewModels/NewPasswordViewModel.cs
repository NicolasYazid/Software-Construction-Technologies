using System;
using System.Windows.Input;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class NewPasswordViewModel : ViewModelBase
    {
        private const string RecoveryTitleKey = "NewPassword_LblTitle";
        private const string ChangeTitleKey = "NewPassword_LblTitleChange";
        private const string PasswordMismatchKey = "Error_ValPasswordMismatch";

        private string _errorMessageKey;

        // The recovery flow reaches this screen precisely when the player no longer knows the password in force.
        // That is why only the change from the profile asks for the current password.
        public NewPasswordViewModel(bool isChangeFromProfile)
        {
            IsChangeFromProfile = isChangeFromProfile;
            TitleKey = RecoveryTitleKey;
            if (isChangeFromProfile)
            {
                TitleKey = ChangeTitleKey;
            }

            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            UpdateCommand = new RelayCommand(Update);
        }

        public event EventHandler PasswordUpdated;

        public bool IsChangeFromProfile { get; private set; }
        public string TitleKey { get; private set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public ICommand UpdateCommand { get; private set; }

        public string ErrorMessageKey
        {
            get { return _errorMessageKey; }
            protected set
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

        protected virtual void SubmitPassword()
        {
            PasswordUpdated?.Invoke(this, EventArgs.Empty);
        }

        // Matching the confirmation is the only check the client resolves on its own, because it sends nothing to the server.
        // The strength rules and the current password itself are verified on the server, as CU-06 requires.
        private void Update()
        {
            ErrorMessageKey = null;
            if (NewPassword != ConfirmPassword)
            {
                ErrorMessageKey = PasswordMismatchKey;
            }
            else
            {
                SubmitPassword();
            }
        }
    }
}
