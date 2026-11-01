using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Ports.Infrastructure;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Ports.Security;

namespace GinRummy.Domain.Services
{
    /// <summary>
    /// Decides who may enter an account and opens the session of whoever may. The sign-in with a password
    /// and its confirmation with the second factor share these rules (CU-02).
    /// </summary>
    public class AccountAccessPolicy
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IClock _clock;

        /// <summary>
        /// Builds the policy with the password hasher and the clock the composition root assembled.
        /// </summary>
        /// <param name="passwordHasher">Compares a typed password with the stored hash.</param>
        /// <param name="clock">Tells the current moment to check the bans and to date the session.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public AccountAccessPolicy(IPasswordHasher passwordHasher, IClock clock)
        {
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
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
        /// Opens a new session for the account and records the moment of the sign-in.
        /// </summary>
        /// <param name="sessions">Repository of the sessions, bound to the transaction of the sign-in.</param>
        /// <param name="account">Account that signs in.</param>
        /// <param name="isPersistent">Whether the player asked to stay signed in.</param>
        /// <returns>The session that was opened.</returns>
        public PlayerSession OpenSession(IPlayerSessionRepository sessions, PlayerAccount account, bool isPersistent)
        {
            DateTime signedInAt = _clock.UtcNow;

            // The database allows a single open session per player, so the previous one is closed first (CU-02 FA-03).
            PlayerSession activeSession = sessions.FindActiveByPlayerId(account.PlayerId);
            if (activeSession != null)
            {
                sessions.End(activeSession.PlayerSessionId, signedInAt);
            }

            PlayerSession openedSession = new PlayerSession(account.PlayerId, isPersistent, signedInAt);
            sessions.Add(openedSession);
            account.RegisterLogin(signedInAt);

            return openedSession;
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
