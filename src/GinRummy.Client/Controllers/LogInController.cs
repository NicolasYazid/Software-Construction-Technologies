using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;

using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Security;

namespace GinRummy.Client.Controllers
{
    // It only reads data, so it needs no transaction.
    public class LogInController
    {
        private const string RequiredFieldMessageKey = "Error_ValRequiredField";
        private const string EmailNotFoundMessageKey = "Error_AuthEmailNotFound";
        private const string WrongPasswordMessageKey = "Error_AuthWrongPassword";
        private const string ServiceUnavailableMessageKey = "Error_SysServiceUnavailable";

        private readonly IPlayerDao _playerDao;
        private readonly IPasswordHasher _passwordHasher;

        public LogInController(IPlayerDao playerDao, IPasswordHasher passwordHasher)
        {
            _playerDao = playerDao;
            _passwordHasher = passwordHasher;
        }

        public LogInResult SignIn(string email, string password)
        {
            LogInResult result = null;

            if (AreAnyFieldsEmpty(email, password))
            {
                result = LogInResult.Failure(RequiredFieldMessageKey);
            }

            if ((result == null) && !IsValidEmailFormat(email))
            {
                // An email that is not even shaped like one cannot match any account.
                // It is therefore reported the same as an email that does not exist.
                result = LogInResult.Failure(EmailNotFoundMessageKey);
            }

            if (result == null)
            {
                result = AuthenticateSafely(email, password);
            }

            return result;
        }

        // Neither field may be empty (FA-02).
        private static bool AreAnyFieldsEmpty(string email, string password)
        {
            return string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(password);
        }

        // The format is checked with MailAddress instead of a hand-written regular expression that the team would have to maintain.
        private static bool IsValidEmailFormat(string email)
        {
            bool isValid;
            try
            {
                _ = new MailAddress(email);
                isValid = true;
            }
            catch (FormatException)
            {
                isValid = false;
            }

            return isValid;
        }

        // A database that cannot be reached must end in a message on the screen.
        // An unhandled exception would leave the window unresponsive instead.
        private LogInResult AuthenticateSafely(string email, string password)
        {
            LogInResult result;
            try
            {
                result = Authenticate(email, password);
            }
            catch (DataException)
            {
                result = LogInResult.Failure(ServiceUnavailableMessageKey);
            }
            catch (SqlException)
            {
                result = LogInResult.Failure(ServiceUnavailableMessageKey);
            }

            return result;
        }

        // The activity asks for one message for an unknown email and another for a wrong password.
        // The email is normalized to lower case to match how it was stored.
        private LogInResult Authenticate(string email, string password)
        {
            LogInResult result;
            Player player = _playerDao.FindByEmail(email.Trim().ToLowerInvariant());
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
