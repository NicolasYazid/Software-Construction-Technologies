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

        public bool IsPermanent
        {
            get { return !RemainingTime.HasValue; }
        }
    }
}
