using System;

namespace GinRummy.Domain.Infrastructure
{
    /// <summary>
    /// Contract the game's logic uses to know the current time, so expiry dates, sanction
    /// periods and timestamps can be tested with a fixed moment instead of the real clock.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the current moment, in UTC.
        /// </summary>
        DateTime UtcNow { get; }
    }
}
