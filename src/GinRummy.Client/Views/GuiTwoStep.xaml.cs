using System;
using System.Windows;
using System.Windows.Threading;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Two-step verification screen (P06). Serves CU-02 FA-04, CU-04 and CU-06, each with its
    /// own instruction key, because one key cannot hold three different values.
    /// </summary>
    public partial class GuiTwoStep : GuiWindowBase
    {
        private const int CodeLifetimeSeconds = 300;
        private const int TimerIntervalSeconds = 1;
        private const string DurationFormat = @"mm\:ss";

        private readonly DispatcherTimer _countdownTimer;
        private readonly TwoStepPurpose _purpose;
        private TimeSpan _codeRemainingTime;

        /// <summary>
        /// Builds the screen for the sign-in flow.
        /// </summary>
        public GuiTwoStep()
            : this(TwoStepPurpose.LogIn)
        {
        }

        /// <summary>
        /// Builds the screen for a given flow.
        /// </summary>
        /// <param name="purpose">Reason why the second factor was requested.</param>
        public GuiTwoStep(TwoStepPurpose purpose)
        {
            InitializeComponent();
            _purpose = purpose;
            _codeRemainingTime = TimeSpan.FromSeconds(CodeLifetimeSeconds);
            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(TimerIntervalSeconds);
            _countdownTimer.Tick += OnCountdownTick;
            _countdownTimer.Start();
            Closed += OnScreenClosed;
            RefreshFormattedText();
        }

        /// <summary>
        /// Rebuilds the instructions and the countdown of the code.
        /// </summary>
        protected override void RefreshFormattedText()
        {
            if (lblInstructions != null)
            {
                lblInstructions.Text = Localization.GetText(ResolveInstructionsKey());
                lblCodeExpiresIn.Text = Localization.Format(
                    "TwoStep_LblCodeExpiresIn",
                    _codeRemainingTime.ToString(DurationFormat, Localization.CurrentCulture));
            }
        }

        private string ResolveInstructionsKey()
        {
            string key = "TwoStep_LblInstructions";
            if (_purpose == TwoStepPurpose.EnableTwoStep)
            {
                key = "TwoStep_LblInstructionsEnable";
            }
            else if (_purpose == TwoStepPurpose.ChangePassword)
            {
                key = "TwoStep_LblInstructionsPassword";
            }

            return key;
        }

        private void OnCountdownTick(object sender, EventArgs e)
        {
            if (_codeRemainingTime > TimeSpan.Zero)
            {
                _codeRemainingTime = _codeRemainingTime.Subtract(
                    TimeSpan.FromSeconds(TimerIntervalSeconds));
            }

            RefreshFormattedText();
        }

        private void OnVerifyClick(object sender, RoutedEventArgs e)
        {
            // The code is checked on the server. The lobby (P11) belongs to a later delivery.
            Close();
        }

        private void OnResendCodeClick(object sender, RoutedEventArgs e)
        {
            _codeRemainingTime = TimeSpan.FromSeconds(CodeLifetimeSeconds);
            RefreshFormattedText();
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _countdownTimer.Stop();
            _countdownTimer.Tick -= OnCountdownTick;
            Closed -= OnScreenClosed;
        }
    }
}
