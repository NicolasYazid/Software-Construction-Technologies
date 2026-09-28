using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;
using GinRummy.Domain.Security;

namespace GinRummy.Client.Controllers
{
    /// <summary>
    /// Orchestrates CU-02 (Sign in) at the scope of this activity: validates the form,
    /// looks up the player by email, and checks the password. It only reads data, so it
    /// needs no transaction.
    /// </summary>
    public class LogInController
    {
        private const string RequiredFieldMessageKey = "Error_ValRequiredField";
        private const string EmailNotFoundMessageKey = "Error_AuthEmailNotFound";
        private const string WrongPasswordMessageKey = "Error_AuthWrongPassword";

        private readonly IPlayerRepository _playerRepository;
        private readonly IPasswordHasher _passwordHasher;

        /// <summary>
        /// Builds the controller with the player repository and the password hasher the
        /// composition root already assembled.
        /// </summary>
        /// <param name="playerRepository">Repository used to look up the player.</param>
        /// <param name="passwordHasher">Adapter that checks the password against its hash.</param>
        public LogInController(IPlayerRepository playerRepository, IPasswordHasher passwordHasher)
        {
            _playerRepository = playerRepository;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Attempts to sign in with an email and a password, or reports what failed.
        /// </summary>
        /// <param name="email">Email written on the form.</param>
        /// <param name="password">Password written on the form.</param>
        /// <returns>The outcome of the attempt.</returns>
        public LogInResult SignIn(string email, string password)
        {
            LogInResult result = null;

            if (AreAnyFieldsEmpty(email, password))
            {
                result = LogInResult.Failure(RequiredFieldMessageKey);
            }

            if (result == null && !IsValidEmailFormat(email))
            {
                // An email that is not even shaped like one cannot match any account, so it
                // is reported the same as an email that does not exist.
                result = LogInResult.Failure(EmailNotFoundMessageKey);
            }

            if (result == null)
            {
                result = Authenticate(email, password);
            }

            return result;
        }

        // FA-02: neither field may be empty.
        private static bool AreAnyFieldsEmpty(string email, string password)
        {
            return string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(password);
        }

        // MailAddress throws FormatException for anything not shaped like an email address,
        // so it validates the format without a hand-written regular expression.
        private static bool IsValidEmailFormat(string email)
        {
            bool isValid;
            try
            {
                MailAddress parsedAddress = new MailAddress(email);
                isValid = true;
            }
            catch (System.FormatException)
            {
                isValid = false;
            }

            return isValid;
        }

        // Looks up the player and checks the password, producing the separate messages the
        // activity asks for. Email is normalized to lower case to match how it was stored.
        private LogInResult Authenticate(string email, string password)
        {
            LogInResult result;
            Player player = _playerRepository.FindByEmail(email.Trim().ToLowerInvariant());
            if (player == null)
            {
                result = LogInResult.Failure(EmailNotFoundMessageKey);
            }
            else if (!_passwordHasher.VerifyPassword(password, player.PasswordHash))
            {
                result = LogInResult.Failure(WrongPasswordMessageKey);
            }
            else
            {
                result = LogInResult.Success(player.Username);
            }

            return result;
        }
    }
}
