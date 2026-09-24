using System;
using System.Windows;
using System.Windows.Threading;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Verification code screen (P03). Implements CU-09 and the verification step of CU-01,
    /// CU-07 and CU-08. Every duration and counter it shows is formatted with the active
    /// culture and never written inside the string.
    /// </summary>
    public partial class GuiVerifyEmail : GuiWindowBase
    {
        private const int CodeLifetimeSeconds = 600;
        private const int ResendDelaySeconds = 60;
        private const int MaximumAttempts = 3;
        private const int TimerIntervalSeconds = 1;
        private const string DurationFormat = @"mm\:ss";

        private readonly DispatcherTimer _countdownTimer;
        private readonly VerificationPurpose _purpose;
        private readonly string _destinationAddress;
        private TimeSpan _codeRemainingTime;
        private TimeSpan _resendRemainingTime;
        private int _remainingAttempts;

        /// <summary>
        /// Builds the screen for the sign-up flow without a known address.
        /// </summary>
        public GuiVerifyEmail()
            : this(VerificationPurpose.AccountSignUp, string.Empty)
        {
        }

        /// <summary>
        /// Builds the screen for a given flow.
        /// </summary>
        /// <param name="purpose">Reason why the code was requested.</param>
        /// <param name="destinationAddress">Address the code was sent to.</param>
        public GuiVerifyEmail(VerificationPurpose purpose, string destinationAddress)
        {
            InitializeComponent();
            _purpose = purpose;
            _destinationAddress = destinationAddress ?? string.Empty;
            _codeRemainingTime = TimeSpan.FromSeconds(CodeLifetimeSeconds);
            _resendRemainingTime = TimeSpan.FromSeconds(ResendDelaySeconds);
            _remainingAttempts = MaximumAttempts;
            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(TimerIntervalSeconds);
            _countdownTimer.Tick += OnCountdownTick;
            _countdownTimer.Start();
            Closed += OnScreenClosed;
            RefreshFormattedText();
        }

        /// <summary>
        /// Rebuilds the heading, the window title, the instructions and every value that
        /// carries a placeholder. The title comes from a different key in each flow, so it
        /// cannot be bound in XAML to a single one.
        /// </summary>
        protected override void RefreshFormattedText()
        {
            if (lblTitle != null)
            {
                string titleKey = ResolveTitleKey();
                lblTitle.Text = Localization.GetText(titleKey);
                Title = Localization.GetText(titleKey);
                lblInstructions.Text = Localization.Format(
                    "VerifyEmail_LblInstructions", _destinationAddress);
                lblCodeExpiresIn.Text = Localization.Format(
                    "VerifyEmail_LblCodeExpiresIn", FormatDuration(_codeRemainingTime));
                lblAttemptsLeft.Text = Localization.Format(
                    "VerifyEmail_LblAttemptsLeft", _remainingAttempts);
                lblResendAvailableIn.Text = Localization.Format(
                    "VerifyEmail_LblResendAvailableIn", FormatDuration(_resendRemainingTime));
            }
        }

        private string ResolveTitleKey()
        {
            string key = "VerifyEmail_LblTitleSignUp";
            if (_purpose == VerificationPurpose.PasswordRecovery)
            {
                key = "VerifyEmail_LblTitleRecovery";
            }
            else if (_purpose == VerificationPurpose.EmailChange)
            {
                key = "VerifyEmail_LblTitleNewEmail";
            }

            return key;
        }

        private string FormatDuration(TimeSpan duration)
        {
            return duration.ToString(DurationFormat, Localization.CurrentCulture);
        }

        private void OnCountdownTick(object sender, EventArgs e)
        {
            TimeSpan step = TimeSpan.FromSeconds(TimerIntervalSeconds);
            if (_codeRemainingTime > TimeSpan.Zero)
            {
                _codeRemainingTime = _codeRemainingTime.Subtract(step);
            }

            if (_resendRemainingTime > TimeSpan.Zero)
            {
                _resendRemainingTime = _resendRemainingTime.Subtract(step);
            }

            RefreshFormattedText();
        }

        private void OnVerifyClick(object sender, RoutedEventArgs e)
        {
            // The code is checked on the server, as CU-09 requires. The screen only advances
            // so that the navigation of the prototype can be walked through.
            if (_purpose == VerificationPurpose.PasswordRecovery)
            {
                GuiNewPassword newPassword = new GuiNewPassword(false);
                newPassword.Owner = Owner;
                newPassword.Show();
            }
            else
            {
                GuiAccountCreated accountCreated = new GuiAccountCreated();
                accountCreated.Owner = Owner;
                accountCreated.Show();
            }

            Close();
        }

        private void OnResendCodeClick(object sender, RoutedEventArgs e)
        {
            _codeRemainingTime = TimeSpan.FromSeconds(CodeLifetimeSeconds);
            _resendRemainingTime = TimeSpan.FromSeconds(ResendDelaySeconds);
            RefreshFormattedText();
        }

        private void OnVerifyLaterClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _countdownTimer.Stop();
            _countdownTimer.Tick -= OnCountdownTick;
            Closed -= OnScreenClosed;
        }
    }
}
