using System;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    // It notifies the changes of its remaining time because the sanctions screen counts it down while it is open (CU-33).
    public sealed class SanctionViewModel : ViewModelBase
    {
        private TimeSpan _remainingTime;

        public string ReasonName { get; set; }
        public bool IsActive { get; set; }
        public DateTime ServedAt { get; set; }

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
