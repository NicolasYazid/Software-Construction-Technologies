using System;
using System.ComponentModel;

namespace GinRummy.Client.Models
{
    /// <summary>
    /// Sanction of the player as the sanctions screen lists it (CU-33). It announces the
    /// changes of its remaining time, which the screen counts down while it is open.
    /// </summary>
    public sealed class SanctionDto : INotifyPropertyChanged
    {
        private TimeSpan _remainingTime;

        /// <summary>
        /// Occurs when the remaining time of the sanction changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the name of the reason of the ban, already in the active language.
        /// </summary>
        public string ReasonName { get; set; }
        /// <summary>
        /// Gets or sets whether the sanction is still in force.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the time left until the sanction expires, while it is in force.
        /// </summary>
        public TimeSpan RemainingTime
        {
            get { return _remainingTime; }
            set
            {
                _remainingTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingTime)));
            }
        }

        /// <summary>
        /// Gets or sets the day the sanction was served, once it is no longer in force.
        /// </summary>
        public DateTime ServedAt { get; set; }
    }
}
