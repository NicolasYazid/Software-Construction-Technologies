using System;
using System.Globalization;

namespace GinRummy.Domain.Entities.Authentication
{
    public sealed class AccessTokenOwner : IEquatable<AccessTokenOwner>
    {
        private const int HashMultiplier = 397;

        private AccessTokenOwner(int? playerAccountId, int? guestSessionId)
        {
            PlayerAccountId = playerAccountId;
            GuestSessionId = guestSessionId;
        }

        public int? PlayerAccountId { get; private set; }

        public int? GuestSessionId { get; private set; }

        public bool IsGuest
        {
            get { return GuestSessionId.HasValue; }
        }

        public static AccessTokenOwner OfPlayerAccount(int playerAccountId)
        {
            ValidateIdentifier(playerAccountId, nameof(playerAccountId));

            return new AccessTokenOwner(playerAccountId, null);
        }

        public static AccessTokenOwner OfGuestSession(int guestSessionId)
        {
            ValidateIdentifier(guestSessionId, nameof(guestSessionId));

            return new AccessTokenOwner(null, guestSessionId);
        }

        public bool Equals(AccessTokenOwner other)
        {
            bool isSameOwner = (other != null)
                && (PlayerAccountId == other.PlayerAccountId)
                && (GuestSessionId == other.GuestSessionId);

            return isSameOwner;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AccessTokenOwner);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (PlayerAccountId.GetValueOrDefault() * HashMultiplier) ^ GuestSessionId.GetValueOrDefault();
            }
        }

        public override string ToString()
        {
            string kind = IsGuest ? "GuestSession" : "PlayerAccount";
            int identifier = IsGuest ? GuestSessionId.Value : PlayerAccountId.Value;

            return string.Format(CultureInfo.InvariantCulture, "{0} {1}", kind, identifier);
        }

        private static void ValidateIdentifier(int identifier, string parameterName)
        {
            if (identifier <= 0)
            {
                throw new ArgumentException("The identifier must be positive.", parameterName);
            }
        }
    }
}
