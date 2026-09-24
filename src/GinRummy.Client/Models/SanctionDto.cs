using System;

namespace GinRummy.Client.Models
{
    /// <summary>
    /// Sanction of the player as the sanctions screen lists it (CU-33).
    /// </summary>
    public sealed class SanctionDto
    {
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
        public TimeSpan RemainingTime { get; set; }
        /// <summary>
        /// Gets or sets the day the sanction was served, once it is no longer in force.
        /// </summary>
        public DateTime ServedAt { get; set; }
    }
}
