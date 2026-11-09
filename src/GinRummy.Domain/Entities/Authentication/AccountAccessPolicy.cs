using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerModeration;
using GinRummy.Domain.Ports.Authentication;
using GinRummy.Domain.Shared;

namespace GinRummy.Domain.Entities.Authentication
{
    /// <summary>
    /// Decides who may enter an account, grants the access token to whoever may and tells which player a presented
    /// token belongs to. The sign-in with a password and its confirmation with the second factor share these rules (CU-02).
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
        /// <param name="account">Account whose credentials matched, with its preferences loaded.</param>
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
                string reasonName = blockingBan.ReasonBan.GetNameFor(account.PlayerPreferences.LocaleId);
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

            return _tokenSigner.Sign(AccessTokenOwner.OfPlayerAccount(account.PlayerAccountId), signedInAt, expiresAt);
        }

        /// <summary>
        /// Checks that a new password may replace the one an account has.
        /// </summary>
        /// <param name="account">Account that changes its password.</param>
        /// <param name="newPassword">New password exactly as the player typed it.</param>
        /// <exception cref="WeakPasswordException">Thrown when the new password does not meet the policy.</exception>
        /// <exception cref="SamePasswordException">Thrown when the new password is the one the account already has.</exception>
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

        /// <summary>
        /// Replaces the password of an account, after the same checks as EnsurePasswordCanReplace.
        /// </summary>
        /// <param name="account">Account that changes its password.</param>
        /// <param name="newPassword">New password exactly as the player typed it.</param>
        /// <exception cref="WeakPasswordException">Thrown when the new password does not meet the policy.</exception>
        /// <exception cref="SamePasswordException">Thrown when the new password is the one the account already has.</exception>
        public void ReplacePassword(PlayerAccount account, string newPassword)
        {
            EnsurePasswordCanReplace(account, newPassword);
            account.ChangePasswordHash(_passwordHasher.HashPassword(new Password(newPassword)));
        }

        /// <summary>
        /// Creates the session of a guest, dated at the current moment.
        /// </summary>
        /// <param name="temporalDisplayName">Name the other players see while the guest is in the lobby.</param>
        /// <returns>The new session, not stored yet.</returns>
        public GuestSession CreateGuestSession(string temporalDisplayName)
        {
            return new GuestSession(temporalDisplayName, _clock.UtcNow);
        }

        /// <summary>
        /// Grants the access token of a guest, which lasts as long as the one of a player who does not stay signed in.
        /// </summary>
        /// <param name="guest">Session of the guest, already stored so it has its identifier.</param>
        /// <returns>The access token that identifies the guest.</returns>
        public AccessToken GrantGuestAccess(GuestSession guest)
        {
            DateTime grantedAt = _clock.UtcNow;

            return _tokenSigner.Sign(AccessTokenOwner.OfGuestSession(guest.GuestSessionId), grantedAt, grantedAt.AddHours(TokenLifetimeHours));
        }

        /// <summary>
        /// Tells who an access token belongs to, after checking its signature and its expiry at the current moment.
        /// </summary>
        /// <param name="accessToken">Encoded token, exactly as the client presented it.</param>
        /// <returns>The owner the token was granted to: a player with an account or a guest.</returns>
        /// <exception cref="InvalidAccessTokenException">Thrown when the token is malformed, altered or expired.</exception>
        public AccessTokenOwner IdentifyByAccessToken(string accessToken)
        {
            // The owner comes from the token the server signed, never from what a client says about itself.
            AccessTokenOwner owner = _tokenSigner.ReadAccessTokenOwner(accessToken, _clock.UtcNow);

            return owner;
        }

        /// <summary>
        /// Tells which player an access token belongs to, after checking its signature and its expiry at the current moment.
        /// </summary>
        /// <param name="accessToken">Encoded token, exactly as the client presented it.</param>
        /// <returns>The identifier of the player the token was granted to.</returns>
        /// <exception cref="InvalidAccessTokenException">
        /// Thrown when the token is malformed, altered or expired, or when it belongs to a guest.
        /// </exception>
        public int IdentifyPlayerByAccessToken(string accessToken)
        {
            AccessTokenOwner owner = IdentifyByAccessToken(accessToken);

            // A guest has no account, so its token never reaches what only a player may do (PRM-01).
            if (owner.IsGuest)
            {
                throw new InvalidAccessTokenException("The access token belongs to a guest.");
            }

            return owner.PlayerAccountId.Value;
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
