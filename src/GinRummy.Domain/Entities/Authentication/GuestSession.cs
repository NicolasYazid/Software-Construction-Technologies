using System;

namespace GinRummy.Domain.Entities.Authentication
{
    public class GuestSession
    {
        public const int TemporalDisplayNameMaxLength = 20;

        public GuestSession(string temporalDisplayName, DateTime createdAt)
        {
            ValidateDisplayName(temporalDisplayName);

            TemporalDisplayName = temporalDisplayName.Trim();
            CreatedAt = createdAt;
        }

        protected GuestSession()
        {
        }

        public int GuestSessionId { get; private set; }

        public string TemporalDisplayName { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private static void ValidateDisplayName(string temporalDisplayName)
        {
            if (string.IsNullOrWhiteSpace(temporalDisplayName))
            {
                throw new ArgumentException("Display name is required.", nameof(temporalDisplayName));
            }

            if (temporalDisplayName.Trim().Length > TemporalDisplayNameMaxLength)
            {
                throw new ArgumentException("Display name is too long.", nameof(temporalDisplayName));
            }
        }
    }
}
