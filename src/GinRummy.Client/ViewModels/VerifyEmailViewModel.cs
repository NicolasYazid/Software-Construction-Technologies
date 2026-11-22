using System;
using System.Windows.Input;

using GinRummy.Client.Models;
using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class VerifyEmailViewModel : ViewModelBase
    {
        // The server issues every verification code for ten minutes and allows three wrong attempts (CU-09).
        protected const int MaximumAttempts = 3;
        protected const int AttemptJustUsed = 1;
        public const int CodeLifetimeSeconds = 600;
        private const int ResendDelaySeconds = 60;
        private const string SignUpTitleKey = "VerifyEmail_LblTitleSignUp";
        private const string RecoveryTitleKey = "VerifyEmail_LblTitleRecovery";
        private const string NewEmailTitleKey = "VerifyEmail_LblTitleNewEmail";

        private int _remainingAttempts;
        private string _errorMessageKey;
        private bool _isAlreadyVerified;
        private bool _isCodeNotSentShown;

        // The title comes from a different key in each flow, so the view binds the key instead of a single text.
        public VerifyEmailViewModel(VerificationPurpose purpose, string destinationAddress)
        {
            Purpose = purpose;
            TitleKey = ResolveTitleKey(purpose);
            DestinationAddress = destinationAddress ?? string.Empty;
            _remainingAttempts = MaximumAttempts;
            Code = string.Empty;
            CodeCountdown = new CountdownViewModel(TimeSpan.FromSeconds(CodeLifetimeSeconds));
            ResendCountdown = new CountdownViewModel(TimeSpan.FromSeconds(ResendDelaySeconds));
            CodeCountdown.Start();
            ResendCountdown.Start();
            VerifyCommand = new RelayCommand(Verify);
            ResendCodeCommand = new RelayCommand(ResendCode);
        }

        public event EventHandler CodeVerified;

        public event EventHandler<ErrorDialogKind> ErrorDialogRequested;

        public VerificationPurpose Purpose { get; private set; }
        public string TitleKey { get; private set; }
        public string DestinationAddress { get; private set; }
        public string Code { get; set; }
        public CountdownViewModel CodeCountdown { get; private set; }
        public CountdownViewModel ResendCountdown { get; private set; }
        public ICommand VerifyCommand { get; private set; }
        public ICommand ResendCodeCommand { get; private set; }

        public int RemainingAttempts
        {
            get { return _remainingAttempts; }
            protected set
            {
                _remainingAttempts = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessageKey
        {
            get { return _errorMessageKey; }
            protected set
            {
                _errorMessageKey = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasErrorMessage));
            }
        }

        public bool HasErrorMessage
        {
            get { return !string.IsNullOrEmpty(_errorMessageKey) ? true : false; }
        }

        public bool IsAlreadyVerified
        {
            get { return _isAlreadyVerified; }
            protected set
            {
                _isAlreadyVerified = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsVerificationPending));
            }
        }

        public bool IsVerificationPending
        {
            get { return !_isAlreadyVerified ? true : false; }
        }

        public bool IsCodeNotSentShown
        {
            get { return _isCodeNotSentShown; }
            protected set
            {
                _isCodeNotSentShown = value;
                OnPropertyChanged();
            }
        }

        public void StopCountdowns()
        {
            CodeCountdown.Stop();
            ResendCountdown.Stop();
        }

        // The code is checked on the server, as CU-09 requires, so this only ends the screen once the code was accepted.
        protected virtual void Verify()
        {
            StopCountdowns();
            CodeVerified?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void ResendCode()
        {
            CodeCountdown.Restart(TimeSpan.FromSeconds(CodeLifetimeSeconds));
            ResendCountdown.Restart(TimeSpan.FromSeconds(ResendDelaySeconds));
        }

        // The server tells how long to wait when it refuses a new code, and the screen counts that wait instead of its own.
        protected void RestartResendCountdown(TimeSpan wait)
        {
            ResendCountdown.Restart(wait);
        }

        protected void OnErrorDialogRequested(ErrorDialogKind kind)
        {
            ErrorDialogRequested?.Invoke(this, kind);
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
    }
}
