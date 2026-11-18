using System;
using System.Windows.Threading;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class ReconnectingViewModel : ViewModelBase
    {
        private const int ReconnectionWindowSeconds = 60;
        private const int SecondsBetweenAttempts = 10;
        private readonly Func<bool> _tryReconnect;
        private readonly DispatcherTimer _attemptTimer;
        private int _attemptsLeft;

        public ReconnectingViewModel(Func<bool> tryReconnect)
        {
            _tryReconnect = tryReconnect ?? throw new ArgumentNullException(nameof(tryReconnect));
            _attemptsLeft = ReconnectionWindowSeconds / SecondsBetweenAttempts;
            _attemptTimer = new DispatcherTimer();
            _attemptTimer.Interval = TimeSpan.FromSeconds(SecondsBetweenAttempts);
            _attemptTimer.Tick += OnAttemptTimerTick;
            Countdown = new CountdownViewModel(TimeSpan.FromSeconds(ReconnectionWindowSeconds));
        }

        public event EventHandler Reconnected;
        public event EventHandler GaveUp;

        public CountdownViewModel Countdown { get; private set; }

        public void Start()
        {
            Countdown.Start();
            _attemptTimer.Start();
        }

        public void Stop()
        {
            _attemptTimer.Stop();
            Countdown.Stop();
        }

        private void OnAttemptTimerTick(object sender, EventArgs e)
        {
            _attemptsLeft--;
            if (_tryReconnect())
            {
                Stop();
                Reconnected?.Invoke(this, EventArgs.Empty);
            }
            else if (_attemptsLeft == 0)
            {
                Stop();
                GaveUp?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
