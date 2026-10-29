using System;
using System.Windows.Input;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class VerifyEmailViewModel : ViewModelBase
    {
        private const int CodeLifetimeSeconds = 600;
        private const int ResendDelaySeconds = 60;
        private const int MaximumAttempts = 3;
        private const string SignUpTitleKey = "VerifyEmail_LblTitleSignUp";
        private const string RecoveryTitleKey = "VerifyEmail_LblTitleRecovery";
        private const string NewEmailTitleKey = "VerifyEmail_LblTitleNewEmail";

        // The title comes from a different key in each flow, so the view binds the key instead of a single text.
        public VerifyEmailViewModel(VerificationPurpose purpose, string destinationAddress)
        {
            Purpose = purpose;
            TitleKey = ResolveTitleKey(purpose);
            DestinationAddress = destinationAddress ?? string.Empty;
            RemainingAttempts = MaximumAttempts;
            Code = string.Empty;
            CodeCountdown = new CountdownViewModel(TimeSpan.FromSeconds(CodeLifetimeSeconds));
            ResendCountdown = new CountdownViewModel(TimeSpan.FromSeconds(ResendDelaySeconds));
            CodeCountdown.Start();
            ResendCountdown.Start();
            VerifyCommand = new RelayCommand(Verify);
            ResendCodeCommand = new RelayCommand(ResendCode);
        }

        public event EventHandler CodeVerified;

        public VerificationPurpose Purpose { get; private set; }
        public string TitleKey { get; private set; }
        public string DestinationAddress { get; private set; }
        public int RemainingAttempts { get; private set; }
        public string Code { get; set; }
        public CountdownViewModel CodeCountdown { get; private set; }
        public CountdownViewModel ResendCountdown { get; private set; }
        public ICommand VerifyCommand { get; private set; }
        public ICommand ResendCodeCommand { get; private set; }

        public void StopCountdowns()
        {
            CodeCountdown.Stop();
            ResendCountdown.Stop();
        }

        private static string ResolveTitleKey(VerificationPurpose purpose)
        {
            string titleKey = SignUpTitleKey;
            if (purpose == VerificationPurpose.PasswordRecovery)
            {
                titleKey = RecoveryTitleKey;
            }
            else if (purpose == VerificationPurpose.EmailChange)
            {
                titleKey = NewEmailTitleKey;
            }

            return titleKey;
        }

        // The code is checked on the server, as CU-09 requires, so the screen only advances the navigation of the prototype.
        private void Verify()
        {
            StopCountdowns();
            CodeVerified?.Invoke(this, EventArgs.Empty);
        }

        private void ResendCode()
        {
            CodeCountdown.Restart(TimeSpan.FromSeconds(CodeLifetimeSeconds));
            ResendCountdown.Restart(TimeSpan.FromSeconds(ResendDelaySeconds));
        }
    }
}
