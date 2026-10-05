using System;
using System.ComponentModel;

namespace GinRummy.Client.Models
{
    // Sanction of the player as the sanctions screen lists it (CU-33). It announces the changes
    // of its remaining time, which the screen counts down while it is open.
    public sealed class SanctionDto : INotifyPropertyChanged
    {
        private TimeSpan _remainingTime;

        public event PropertyChangedEventHandler PropertyChanged;

        public string ReasonName { get; set; }
        public bool IsActive { get; set; }

        public TimeSpan RemainingTime
        {
            get { return _remainingTime; }
            set
            {
                _remainingTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingTime)));
            }
        }

        public DateTime ServedAt { get; set; }
    }
}
