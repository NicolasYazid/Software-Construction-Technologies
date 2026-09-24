using System;
using System.Windows;
using System.Windows.Threading;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Suspended account screen (P09). Serves CU-02 EX-04, CU-22 EX-05, CU-23 EX-09 and
    /// CU-33 FA-04.
    /// </summary>
    public partial class GuiAccountSuspended : GuiWindowBase
    {
        private const int TimerIntervalSeconds = 1;
        private const string TimeRemainingKey = "AccountSuspended_LblTimeRemaining";

        private readonly DispatcherTimer _countdownTimer;
        private TimeSpan _remainingTime;

        /// <summary>
        /// Builds the screen for a suspension that is still in force.
        /// </summary>
        /// <param name="reasonName">Name of the reason of the ban, already in the active
        /// language.</param>
        /// <param name="remainingTime">Time left until the ban expires.</param>
        public GuiAccountSuspended(string reasonName, TimeSpan remainingTime)
        {
            InitializeComponent();
            lblReasonBan.Text = reasonName;
            _remainingTime = remainingTime;
            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(TimerIntervalSeconds);
            _countdownTimer.Tick += OnCountdownTick;
            _countdownTimer.Start();
            Closed += OnScreenClosed;
            RefreshFormattedText();
        }

        /// <summary>
        /// Rebuilds the remaining time, which carries a placeholder.
        /// </summary>
        protected override void RefreshFormattedText()
        {
            if (lblTimeRemaining != null)
            {
                lblTimeRemaining.Text = Localization.Format(
                    TimeRemainingKey,
                    DurationCommon.ToClock(_remainingTime, Localization.CurrentCulture));
            }
        }

        private void OnCountdownTick(object sender, EventArgs e)
        {
            if (_remainingTime > TimeSpan.Zero)
            {
                _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(TimerIntervalSeconds));
            }

            RefreshFormattedText();
        }

        private void OnLogOutClick(object sender, RoutedEventArgs e)
        {
            // A suspended account never gets a session (CU-02 EX-04), so leaving only takes
            // the player back to the screen that opened this one.
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
