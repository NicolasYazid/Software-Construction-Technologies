using System;

namespace GinRummy.Client.Models
{
    public sealed class AccountRestriction
    {
        public AccountRestriction(string reasonName, TimeSpan? remainingTime)
        {
            ReasonName = reasonName;
            RemainingTime = remainingTime;
        }

        public string ReasonName { get; private set; }
        public TimeSpan? RemainingTime { get; private set; }

        // A ban without an end is permanent and leads to the banned account screen (CU-02 EX-05).
        public bool IsPermanent
        {
            get { return !RemainingTime.HasValue; }
        }
    }
}
