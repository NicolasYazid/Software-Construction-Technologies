using System;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    // It notifies the changes of its remaining time because the sanctions screen counts it down while it is open (CU-33).
    public sealed class SanctionViewModel : ViewModelBase
    {
        private TimeSpan _remainingTime;

        public SanctionViewModel(string reasonName, TimeSpan remainingTime)
        {
            ReasonName = reasonName ?? throw new ArgumentNullException(nameof(reasonName));
            IsActive = true;
            _remainingTime = remainingTime;
        }

        public SanctionViewModel(string reasonName, DateTime servedAt)
        {
            ReasonName = reasonName ?? throw new ArgumentNullException(nameof(reasonName));
            ServedAt = servedAt;
        }

        public string ReasonName { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime ServedAt { get; private set; }

        public TimeSpan RemainingTime
        {
            get { return _remainingTime; }
            set
            {
                _remainingTime = value;
                RaisePropertyChanged();
            }
        }
    }
}
