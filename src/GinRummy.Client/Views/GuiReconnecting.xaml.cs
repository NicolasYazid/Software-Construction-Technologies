using System;
using System.Windows;
using System.Windows.Threading;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Reconnection notice (P25). Opened by CU-26 EX-03 and EX-05 and by the NET-03
    /// exceptions of the rest of the use cases.
    /// </summary>
    public partial class GuiReconnecting : GuiModalBase
    {
        // The length of the reconnection window is still an open decision of the team; this
        // value only drives the countdown until the server supplies the real one.
        private const int ReconnectionWindowSeconds = 60;
        private const int TimerIntervalSeconds = 1;
        private const string TimeRemainingKey = "Reconnecting_LblTimeRemaining";

        private readonly DispatcherTimer _countdownTimer;
        private TimeSpan _remainingTime;

        /// <summary>
        /// Builds the notice and starts the countdown of the reconnection window.
        /// </summary>
        public GuiReconnecting()
        {
            InitializeComponent();
            _remainingTime = TimeSpan.FromSeconds(ReconnectionWindowSeconds);
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

        private void OnCancelClick(object sender, RoutedEventArgs e)
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
