using System;
using System.Windows.Threading;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    // The time is counted on the dispatcher of the interface, so every tick may change a bound property directly.
    public class CountdownViewModel : ViewModelBase
    {
        private const int StepSeconds = 1;

        private readonly DispatcherTimer _timer;
        private TimeSpan _remainingTime;

        public CountdownViewModel(TimeSpan duration)
        {
            _remainingTime = duration;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(StepSeconds);
            _timer.Tick += OnTimerTick;
        }

        public TimeSpan RemainingTime
        {
            get { return _remainingTime; }
            private set
            {
                _remainingTime = value;
                RaisePropertyChanged();
            }
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Restart(TimeSpan duration)
        {
            RemainingTime = duration;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            TimeSpan remainingTime = _remainingTime.Subtract(_timer.Interval);
            if (remainingTime <= TimeSpan.Zero)
            {
                remainingTime = TimeSpan.Zero;
                _timer.Stop();
            }

            RemainingTime = remainingTime;
        }
    }
}
