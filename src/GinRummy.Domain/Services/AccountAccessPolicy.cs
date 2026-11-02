using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerModeration;
using GinRummy.Domain.Entities.Players;
using GinRummy.Domain.Ports.Infrastructure;
using GinRummy.Domain.Ports.Security;

namespace GinRummy.Domain.Services
{
    /// <summary>
    /// Decides who may enter an account and grants the access token to whoever may. The sign-in with a
    /// password and its confirmation with the second factor share these rules (CU-02).
    /// </summary>
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
                string reasonName = blockingBan.ReasonBan.GetNameFor(account.Player.LocaleId);
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

            return _tokenSigner.Sign(account.PlayerId, signedInAt, expiresAt);
        }

        public int IdentifyPlayer(string accessToken)
        {
            int playerId = _tokenSigner.ReadPlayerId(accessToken, _clock.UtcNow);

            return playerId;
        }

        // A permanent ban comes before a temporary one, and the temporary ban that ends last comes before the others.
        // The player is therefore told the real end of the restriction when several bans are in force at once.
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
