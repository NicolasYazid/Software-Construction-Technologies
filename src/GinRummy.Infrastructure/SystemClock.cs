using System;

using GinRummy.Domain.Ports.Infrastructure;

namespace GinRummy.Infrastructure
{
    /// <summary>
    /// Fulfils IClock with the clock of the machine that runs the server, so the domain, the use cases
    /// and the data access never read the system clock themselves.
    /// </summary>
    public class SystemClock : IClock
    {
        /// <summary>
        /// Gets the current moment of the server, in UTC.
        /// </summary>
        public DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }
    }
}
