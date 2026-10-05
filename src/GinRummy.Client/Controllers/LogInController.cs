using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Security;

namespace GinRummy.Client.Controllers
{
    // Orchestrates CU-02 (Sign in) at the scope of this activity: validates the form, looks up
    // the player by email, and checks the password. It only reads data, so it needs no
    // transaction.
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
                // An email that is not even shaped like one cannot match any account, so it
                // is reported the same as an email that does not exist.
                result = LogInResult.Failure(EmailNotFoundMessageKey);
            }

            if (result == null)
            {
                result = AuthenticateSafely(email, password);
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

        // A database that cannot be reached must end in a message on the screen, not in an
        // exception that leaves the window unresponsive.
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

        // Looks up the player and checks the password, producing the separate messages the
        // activity asks for. Email is normalized to lower case to match how it was stored.
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
