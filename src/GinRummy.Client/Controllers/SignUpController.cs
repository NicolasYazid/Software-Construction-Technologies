using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;

using GinRummy.Client.Localization;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace GinRummy.Client.Controllers
{
    public class SignUpController
    {
        private const string RequiredFieldMessageKey = "Error_ValRequiredField";
        private const string InvalidEmailMessageKey = "Error_ValInvalidEmailFormat";
        private const string FieldLengthMessageKey = "Error_ValFieldLength";
        private const string WeakPasswordMessageKey = "Error_ValWeakPassword";
        private const string EmailAlreadyRegisteredMessageKey = "Error_AuthEmailAlreadyRegistered";
        private const string UnexpectedErrorMessageKey = "Error_SysUnexpected";
        private const string ServiceUnavailableMessageKey = "Error_SysServiceUnavailable";
        // The same limits as the username column of Player and the email column of PlayerAccount.
        private const int MaxUsernameLength = 20;
        private const int MaxEmailLength = 254;
        private const int MinimumPasswordLength = 8;
        private const int VerificationCodeLifetimeMinutes = 10;
        private const int UniqueConstraintViolationErrorNumber = 2627;
        private const int UniqueIndexViolationErrorNumber = 2601;

        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountSecurity _accountSecurity;
        private readonly ILogger<SignUpController> _logger;

        public SignUpController(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountSecurity accountSecurity,
            ILogger<SignUpController> logger)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _accountSecurity = accountSecurity;
            _logger = logger;
        }

        public SignUpResult CreateAccount(AccountForm form)
        {
            SignUpResult result = ValidateForm(form);

            if (result == null)
            {
                result = CreateAccountRecord(form);
            }

            return result;
        }

        // None of the three fields may be empty (FA-01).
        private static bool AreAnyFieldsEmpty(string email, string username, string password)
        {
            return string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(username)
                || string.IsNullOrWhiteSpace(password);
        }

        // The database columns cap these fields, and the Player entity refuses a longer value with an exception.
        // Checking first turns that exception into a message instead of a crash.
        private static bool IsTooLong(string text, int maximumLength)
        {
            return text.Trim().Length > maximumLength;
        }

        // The policy of FA-03 is the draft that GuiSignUp already shows, so both must change together.
        private static bool IsStrongPassword(string password)
        {
            bool hasMinimumLength = password.Length >= MinimumPasswordLength;
            bool hasLetter = password.Any(character => char.IsLetter(character));
            bool hasDigit = password.Any(character => char.IsDigit(character));

            return hasMinimumLength && hasLetter && hasDigit;
        }

        // EF6 wraps the SqlException in two layers, under the DbUpdateException and its InnerException.
        // SQL Server raises error 2627 for a UNIQUE KEY violation and error 2601 for a duplicate key in a unique index.
        // Either one may come back, depending on the constraint.
        private static bool IsUniqueEmailViolation(DbUpdateException databaseUpdateException)
        {
            bool isUniqueViolation = false;
            SqlException sqlException = databaseUpdateException.InnerException?.InnerException as SqlException;
            if (sqlException != null)
            {
                isUniqueViolation = (sqlException.Number == UniqueConstraintViolationErrorNumber)
                    || (sqlException.Number == UniqueIndexViolationErrorNumber);
            }

            return isUniqueViolation;
        }

        private SignUpResult ValidateForm(AccountForm form)
        {
            SignUpResult result = null;

            if (AreAnyFieldsEmpty(form.Email, form.Username, form.Password))
            {
                result = SignUpResult.Failure(RequiredFieldMessageKey);
            }

            if ((result == null) && IsTooLong(form.Username, MaxUsernameLength))
            {
                result = SignUpResult.Failure(FieldLengthMessageKey, MaxUsernameLength);
            }

            if ((result == null) && IsTooLong(form.Email, MaxEmailLength))
            {
                result = SignUpResult.Failure(FieldLengthMessageKey, MaxEmailLength);
            }

            if ((result == null) && !IsValidEmailFormat(form.Email))
            {
                result = SignUpResult.Failure(InvalidEmailMessageKey);
            }

            if ((result == null) && !IsStrongPassword(form.Password))
            {
                result = SignUpResult.Failure(WeakPasswordMessageKey);
            }

            return result;
        }

        // The format of FA-02 is checked with MailAddress instead of a hand-written regular expression the team would have to maintain.
        private bool IsValidEmailFormat(string email)
        {
            bool isValid;
            try
            {
                _ = new MailAddress(email);
                isValid = true;
            }
            catch (FormatException ex)
            {
                _logger.LogDebug(ex, "A sign-up email was rejected because it does not have a valid format.");
                isValid = false;
            }

            return isValid;
        }

        // The player and its verification code share one unit of work so that both are committed or neither is.
        private SignUpResult CreateAccountRecord(AccountForm form)
        {
            SignUpResult result;
            // Opening the unit of work already opens a connection and a transaction.
            // A database that cannot be reached therefore fails here, outside the handler of the unit of work.
            try
            {
                using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
                {
                    result = RegisterAccount(unitOfWork, form);
                }
            }
            catch (DataException ex)
            {
                _logger.LogError(ex, "The database could not be reached to create an account.");
                result = SignUpResult.Failure(ServiceUnavailableMessageKey);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "The database could not be reached to create an account.");
                result = SignUpResult.Failure(ServiceUnavailableMessageKey);
            }

            return result;
        }

        private SignUpResult RegisterAccount(IUnitOfWork unitOfWork, AccountForm form)
        {
            SignUpResult result;
            try
            {
                if (unitOfWork.PlayerAccounts.FindByEmail(form.Email.Trim().ToLowerInvariant()) != null)
                {
                    result = SignUpResult.Failure(EmailAlreadyRegisteredMessageKey);
                }
                else
                {
                    result = SaveNewAccount(unitOfWork, form);
                }
            }
            catch (DbUpdateException ex)
            {
                unitOfWork.Rollback();
                result = ResolveSaveFailure(ex);
            }
            catch (ArgumentException ex)
            {
                // The entities reject any value that the checks above let through.
                // The attempt then ends with a message and an untouched database instead of a crash.
                _logger.LogError(ex, "An entity rejected the sign-up data after it passed validation.");
                unitOfWork.Rollback();
                result = SignUpResult.Failure(UnexpectedErrorMessageKey);
            }

            return result;
        }

        // The email may pass the earlier lookup and still collide when another sign-up saves it first.
        private SignUpResult ResolveSaveFailure(DbUpdateException databaseUpdateException)
        {
            SignUpResult result;
            if (IsUniqueEmailViolation(databaseUpdateException))
            {
                _logger.LogWarning(databaseUpdateException, "The email was registered by another sign-up before this one was saved.");
                result = SignUpResult.Failure(EmailAlreadyRegisteredMessageKey);
            }
            else
            {
                _logger.LogError(databaseUpdateException, "The new account could not be saved.");
                result = SignUpResult.Failure(UnexpectedErrorMessageKey);
            }

            return result;
        }

        private SignUpResult SaveNewAccount(IUnitOfWork unitOfWork, AccountForm form)
        {
            Locale locale = unitOfWork.Locales.FindByCode(form.CultureCode)
                ?? unitOfWork.Locales.FindByCode(LocalizationProvider.DefaultCultureCode);

            Player newPlayer = new Player(form.Username, locale.LocaleId);
            unitOfWork.Players.Add(newPlayer);

            PlayerAccount newAccount = new PlayerAccount(
                newPlayer.PlayerId,
                form.Email,
                _accountSecurity.PasswordHasher.HashPassword(form.Password));
            unitOfWork.PlayerAccounts.Add(newAccount);

            string plainCode = _accountSecurity.CodeGenerator.GenerateCode();
            VerificationCode newCode = new VerificationCode(
                newPlayer.PlayerId,
                VerificationPurpose.CreateAccount,
                _accountSecurity.CodeHasher.ComputeHash(plainCode),
                DateTime.UtcNow.AddMinutes(VerificationCodeLifetimeMinutes));
            unitOfWork.VerificationCodes.Add(newCode);

            unitOfWork.Commit();

            return SignUpResult.Success(plainCode);
        }
    }
}
