using System;
using System.Collections.Generic;
using System.Windows.Threading;

using GinRummy.Client.Mvvm;
using GinRummy.Client.Services;

namespace GinRummy.Client.ViewModels
{
    public class SanctionsViewModel : ViewModelBase
    {
        private const int StepSeconds = 1;

        private readonly SampleDataService _dataService;
        private readonly DispatcherTimer _countdownTimer;
        private IList<SanctionViewModel> _sanctions;
        private TimeSpan _elapsedTime;
        private bool _hasNoSanctions;

        public SanctionsViewModel(SampleDataService dataService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(StepSeconds);
            _countdownTimer.Tick += OnCountdownTick;
            LoadSanctions();
            _countdownTimer.Start();
        }

        public IList<SanctionViewModel> Sanctions
        {
            get { return _sanctions; }
            private set
            {
                _sanctions = value;
                RaisePropertyChanged();
            }
        }

        public bool HasNoSanctions
        {
            get { return _hasNoSanctions; }
            private set
            {
                _hasNoSanctions = value;
                RaisePropertyChanged();
            }
        }

        // The name of the reason arrives already translated, so a change of language asks for the sanctions again (CU-33 step 3).
        // They arrive with the time they had when the screen opened, so the time already counted is taken off.
        public void LoadSanctions()
        {
            IList<SanctionViewModel> sanctions = _dataService.GetSanctions();
            foreach (SanctionViewModel sanction in sanctions)
            {
                sanction.RemainingTime = Shorten(sanction.RemainingTime, _elapsedTime);
            }

            Sanctions = sanctions;
            HasNoSanctions = sanctions.Count == 0;
        }

        public void StopCountdown()
        {
            _countdownTimer.Stop();
        }

        private void OnCountdownTick(object sender, EventArgs e)
        {
            _elapsedTime = _elapsedTime.Add(_countdownTimer.Interval);
            foreach (SanctionViewModel sanction in _sanctions)
            {
                sanction.RemainingTime = Shorten(sanction.RemainingTime, _countdownTimer.Interval);
            }
        }

        private static TimeSpan Shorten(TimeSpan remainingTime, TimeSpan elapsedTime)
        {
            TimeSpan shortened = remainingTime.Subtract(elapsedTime);
            if (shortened < TimeSpan.Zero)
            {
                shortened = TimeSpan.Zero;
            }

            return shortened;
        }
    }
}
