using System;
using System.ComponentModel;

namespace GinRummy.Client.Models
{
    // It notifies the changes of its remaining time because the sanctions screen counts it down while it is open (CU-33).
    public sealed class SanctionDto : INotifyPropertyChanged
    {
        private TimeSpan _remainingTime;

        public event PropertyChangedEventHandler PropertyChanged;

        public string ReasonName { get; set; }
        public bool IsActive { get; set; }
        public DateTime ServedAt { get; set; }

        public TimeSpan RemainingTime
        {
            get { return _remainingTime; }
            set
            {
                _remainingTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingTime)));
            }
        }
    }
}
