using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerModeration;
using GinRummy.Domain.Ports.Authentication;
using GinRummy.Domain.Shared;

namespace GinRummy.Domain.Entities.Authentication
{
    public class AccountAccessPolicy
    {
        public const int TokenLifetimeHours = 8;
        public const int KeepSignedInTokenLifetimeDays = 30;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAccessTokenSigner _tokenSigner;
        private readonly IClock _clock;

        public AccountAccessPolicy(IPasswordHasher passwordHasher, IAccessTokenSigner tokenSigner, IClock clock)
        {
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _tokenSigner = tokenSigner ?? throw new ArgumentNullException(nameof(tokenSigner));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public void EnsureCredentialsMatch(PlayerAccount account, string password)
        {
            bool isMatch = (account != null) && _passwordHasher.VerifyPassword(password, account.PasswordHash);
            if (!isMatch)
            {
                throw new InvalidCredentialsException("The email or the password is not valid.");
            }
        }

        public void EnsureAccessAllowed(PlayerAccount account, IEnumerable<Ban> bans)
        {
            if (!account.IsEmailVerified)
            {
                throw new EmailNotVerifiedException("The email of the account is not verified.");
            }

            Ban blockingBan = FindBlockingBan(bans);
            if (blockingBan != null)
            {
                string reasonName = blockingBan.ReasonBan.GetNameFor(account.PlayerPreferences.LocaleId);
                throw new AccountBannedException("A ban restricts the whole account.", reasonName, blockingBan.ExpiresAt);
            }
        }

        public AccessToken GrantAccess(PlayerAccount account, bool keepSignedIn)
        {
            DateTime signedInAt = _clock.UtcNow;
            DateTime expiresAt = signedInAt.AddHours(TokenLifetimeHours);
            if (keepSignedIn)
            {
                expiresAt = signedInAt.AddDays(KeepSignedInTokenLifetimeDays);
            }

            account.RegisterLogin(signedInAt);

            return _tokenSigner.Sign(AccessTokenOwner.OfPlayerAccount(account.PlayerAccountId), signedInAt, expiresAt);
        }

        public void EnsurePasswordCanReplace(PlayerAccount account, string newPassword)
        {
            if (Password.GetUnmetRequirements(newPassword).Count > 0)
            {
                throw new WeakPasswordException("The new password does not meet the policy.");
            }

            if (_passwordHasher.VerifyPassword(newPassword, account.PasswordHash))
            {
                throw new SamePasswordException("The new password is the one the account already has.");
            }
        }

        public void ReplacePassword(PlayerAccount account, string newPassword)
        {
            EnsurePasswordCanReplace(account, newPassword);
            account.ChangePasswordHash(_passwordHasher.HashPassword(new Password(newPassword)));
        }

        public GuestSession CreateGuestSession(string temporalDisplayName)
        {
            return new GuestSession(temporalDisplayName, _clock.UtcNow);
        }

        public AccessToken GrantGuestAccess(GuestSession guest)
        {
            DateTime grantedAt = _clock.UtcNow;

            return _tokenSigner.Sign(AccessTokenOwner.OfGuestSession(guest.GuestSessionId), grantedAt, grantedAt.AddHours(TokenLifetimeHours));
        }

        public AccessTokenOwner IdentifyByAccessToken(string accessToken)
        {
            AccessTokenOwner owner = _tokenSigner.ReadAccessTokenOwner(accessToken, _clock.UtcNow);

            return owner;
        }

        public int IdentifyPlayerByAccessToken(string accessToken)
        {
            AccessTokenOwner owner = IdentifyByAccessToken(accessToken);

            if (owner.IsGuest)
            {
                throw new InvalidAccessTokenException("The access token belongs to a guest.");
            }

            return owner.PlayerAccountId.Value;
        }

        public bool IsSuspended(IEnumerable<Ban> bans)
        {
            return FindBlockingBan(bans) != null;
        }

        private Ban FindBlockingBan(IEnumerable<Ban> bans)
        {
            DateTime now = _clock.UtcNow;
            Ban blockingBan = bans
                .Where(ban => IsBlocking(ban, now))
                .OrderByDescending(ban => ban.ExpiresAt ?? DateTime.MaxValue)
                .FirstOrDefault();

            return blockingBan;
        }

        private static bool IsBlocking(Ban ban, DateTime moment)
        {
            return ban.ReasonBan.IsAccountRestricted && ban.IsActiveAt(moment);
        }
    }
}
