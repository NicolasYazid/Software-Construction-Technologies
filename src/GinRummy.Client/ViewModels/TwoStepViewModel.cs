using System;
using System.Windows.Input;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class TwoStepViewModel : ViewModelBase
    {
        private const int CodeLifetimeSeconds = 300;
        private const string LogInInstructionsKey = "TwoStep_LblInstructions";
        private const string EnableTwoStepInstructionsKey = "TwoStep_LblInstructionsEnable";
        private const string ChangePasswordInstructionsKey = "TwoStep_LblInstructionsPassword";

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

        // Checking the code belongs to the server, so the screen only advances the navigation of the prototype.
        private void Verify()
        {
            CodeCountdown.Stop();
            CodeVerified?.Invoke(this, EventArgs.Empty);
        }

        private void ResendCode()
        {
            CodeCountdown.Restart(TimeSpan.FromSeconds(CodeLifetimeSeconds));
        }
    }
}
