using System;
using System.Windows.Input;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class RecoverPasswordViewModel : ViewModelBase
    {
        private bool _isCodeSentNoticeShown;

        public RecoverPasswordViewModel()
        {
            Email = string.Empty;
            SendCodeCommand = new RelayCommand(SendCode);
        }

        public event EventHandler CodeRequested;

        public string Email { get; set; }
        public ICommand SendCodeCommand { get; private set; }

        public bool IsCodeSentNoticeShown
        {
            get { return _isCodeSentNoticeShown; }
            private set
            {
                _isCodeSentNoticeShown = value;
                RaisePropertyChanged();
            }
        }

        // CU-08 answers the same way whether or not the address has an account.
        // That way the screen never reveals which addresses are registered.
        private void SendCode()
        {
            IsCodeSentNoticeShown = true;
            CodeRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
