using System.Windows.Input;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class SignUpViewModel : ViewModelBase
    {
        private bool _isPasswordMismatchShown;

        public SignUpViewModel()
        {
            Email = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            CreateAccountCommand = new RelayCommand(CreateAccount);
        }

        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public ICommand CreateAccountCommand { get; private set; }

        public bool IsPasswordMismatchShown
        {
            get { return _isPasswordMismatchShown; }
            private set
            {
                _isPasswordMismatchShown = value;
                RaisePropertyChanged();
            }
        }

        // Matching the password with its confirmation is the only check the client may resolve on its own (CU-01 FA-04).
        // Every other validation belongs to the CU-01 service of the server, which the client does not reach yet.
        private void CreateAccount()
        {
            IsPasswordMismatchShown = Password != ConfirmPassword;
        }
    }
}
