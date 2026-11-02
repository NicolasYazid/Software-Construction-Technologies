using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Domain.Entities;
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
        /// <summary>
        /// Hours an access token lasts when the player did not ask to stay signed in.
        /// </summary>
        public const int TokenLifetimeHours = 8;
        /// <summary>
        /// Days an access token lasts when the player asked to stay signed in.
        /// </summary>
        public const int KeepSignedInTokenLifetimeDays = 30;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAccessTokenSigner _tokenSigner;
        private readonly IClock _clock;

        /// <summary>
        /// Builds the policy with the password hasher, the token signer and the clock the composition root assembled.
        /// </summary>
        /// <param name="passwordHasher">Compares a typed password with the stored hash.</param>
        /// <param name="tokenSigner">Signs the access token of whoever may enter.</param>
        /// <param name="clock">Tells the current moment to check the bans and to date the token.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public AccountAccessPolicy(IPasswordHasher passwordHasher, IAccessTokenSigner tokenSigner, IClock clock)
        {
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _tokenSigner = tokenSigner ?? throw new ArgumentNullException(nameof(tokenSigner));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Checks that an account was found and that the typed password matches it.
        /// </summary>
        /// <param name="account">Account found by the typed email, or null when none has it.</param>
        /// <param name="password">Password exactly as the player typed it.</param>
        /// <exception cref="InvalidCredentialsException">Thrown when there is no account or the password does not match.</exception>
        public void EnsureCredentialsMatch(PlayerAccount account, string password)
        {
            bool isMatch = (account != null) && _passwordHasher.VerifyPassword(password, account.PasswordHash);
            if (!isMatch)
            {
                throw new InvalidCredentialsException("The email or the password is not valid.");
            }
        }

        /// <summary>
        /// Checks that an account whose credentials matched may sign in now.
        /// </summary>
        /// <param name="account">Account whose credentials matched, with its player loaded.</param>
        /// <param name="bans">Every ban the player has received, each with its reason loaded.</param>
        /// <exception cref="EmailNotVerifiedException">Thrown when the email of the account is not verified.</exception>
        /// <exception cref="AccountBannedException">Thrown when a ban that restricts the whole account is in force.</exception>
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

        /// <summary>
        /// Grants the access token of the account and records the moment of the sign-in in the account.
        /// </summary>
        /// <param name="account">Account that signs in.</param>
        /// <param name="keepSignedIn">Whether the player asked to stay signed in.</param>
        /// <returns>The signed access token.</returns>
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
