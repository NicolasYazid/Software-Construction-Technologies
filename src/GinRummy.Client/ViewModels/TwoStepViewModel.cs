using System;
using System.Windows.Input;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class TwoStepViewModel : ViewModelBase
    {
        // The server issues every verification code for ten minutes (CU-04 step 4).
        private const int CodeLifetimeSeconds = 600;
        private const string LogInInstructionsKey = "TwoStep_LblInstructions";
        private const string EnableTwoStepInstructionsKey = "TwoStep_LblInstructionsEnable";
        private const string ChangePasswordInstructionsKey = "TwoStep_LblInstructionsPassword";

        private string _errorMessageKey;

        // CU-02 FA-04, CU-04 and CU-06 each have their own instruction key, because one key cannot hold three different values.
        public TwoStepViewModel(TwoStepPurpose purpose)
        {
            Purpose = purpose;
            InstructionsKey = ResolveInstructionsKey(purpose);
            Code = string.Empty;
            CodeCountdown = new CountdownViewModel(TimeSpan.FromSeconds(CodeLifetimeSeconds));
            CodeCountdown.Start();
            VerifyCommand = new RelayCommand(Verify);
            ResendCodeCommand = new RelayCommand(ResendCode);
        }

        public event EventHandler CodeVerified;

        public TwoStepPurpose Purpose { get; private set; }
        public string InstructionsKey { get; private set; }
        public string Code { get; set; }
        public CountdownViewModel CodeCountdown { get; private set; }
        public ICommand VerifyCommand { get; private set; }
        public ICommand ResendCodeCommand { get; private set; }

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

        // Checking the code belongs to the server, so the screens whose service does not exist yet only advance the navigation.
        protected virtual void Verify()
        {
            CodeCountdown.Stop();
            CodeVerified?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void ResendCode()
        {
            CodeCountdown.Restart(TimeSpan.FromSeconds(CodeLifetimeSeconds));
        }

        private static string ResolveInstructionsKey(TwoStepPurpose purpose)
        {
            string instructionsKey = LogInInstructionsKey;
            if (purpose == TwoStepPurpose.EnableTwoStep)
            {
                instructionsKey = EnableTwoStepInstructionsKey;
            }
            else if (purpose == TwoStepPurpose.ChangePassword)
            {
                instructionsKey = ChangePasswordInstructionsKey;
            }

            return instructionsKey;
        }
    }
}
