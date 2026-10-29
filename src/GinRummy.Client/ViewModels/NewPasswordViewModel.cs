using System;
using System.Windows.Input;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class NewPasswordViewModel : ViewModelBase
    {
        private const string RecoveryTitleKey = "NewPassword_LblTitle";
        private const string ChangeTitleKey = "NewPassword_LblTitleChange";

        private bool _isPasswordMismatchShown;

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

        public bool IsPasswordMismatchShown
        {
            get { return _isPasswordMismatchShown; }
            private set
            {
                _isPasswordMismatchShown = value;
                RaisePropertyChanged();
            }
        }

        // Matching the confirmation is the only check the client resolves on its own, because it sends nothing to the server.
        // The strength rules and the current password itself are verified on the server, as CU-06 requires.
        private void Update()
        {
            bool isPasswordConfirmed = NewPassword == ConfirmPassword;
            IsPasswordMismatchShown = !isPasswordConfirmed;
            if (isPasswordConfirmed)
            {
                PasswordUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
