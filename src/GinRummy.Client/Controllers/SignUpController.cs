using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;

using GinRummy.Client.Localization;
using GinRummy.Domain.Daos;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Security;

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
        private const int MinimumPasswordLength = 8;
        private const int VerificationCodeLifetimeMinutes = 10;
        private const int UniqueConstraintViolationErrorNumber = 2627;
        private const int UniqueIndexViolationErrorNumber = 2601;

        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IVerificationCodeGenerator _codeGenerator;
        private readonly IVerificationCodeHasher _codeHasher;

        public SignUpController(
            Func<IUnitOfWork> unitOfWorkFactory,
            IPasswordHasher passwordHasher,
            IVerificationCodeGenerator codeGenerator,
            IVerificationCodeHasher codeHasher)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _passwordHasher = passwordHasher;
            _codeGenerator = codeGenerator;
            _codeHasher = codeHasher;
        }

        public SignUpResult CreateAccount(string email, string username, string password, string activeCultureCode)
        {
            AccountForm form = new AccountForm
            {
                Email = email,
                Username = username,
                Password = password,
                CultureCode = activeCultureCode
            };
            SignUpResult result = ValidateForm(form);

            if (result == null)
            {
                result = CreateAccountRecord(form);
            }

            return result;
        }

        private static SignUpResult ValidateForm(AccountForm form)
        {
            SignUpResult result = null;

            if (AreAnyFieldsEmpty(form.Email, form.Username, form.Password))
            {
                result = SignUpResult.Failure(RequiredFieldMessageKey);
            }

            if ((result == null) && IsTooLong(form.Username, Player.MaxUsernameLength))
            {
                result = SignUpResult.Failure(FieldLengthMessageKey, Player.MaxUsernameLength);
            }

            if ((result == null) && IsTooLong(form.Email, Player.MaxEmailLength))
            {
                result = SignUpResult.Failure(FieldLengthMessageKey, Player.MaxEmailLength);
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

        // The format of FA-02 is checked with MailAddress instead of a hand-written regular expression the team would have to maintain.
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
            catch (DataException)
            {
                result = SignUpResult.Failure(ServiceUnavailableMessageKey);
            }
            catch (SqlException)
            {
                result = SignUpResult.Failure(ServiceUnavailableMessageKey);
            }

            return result;
        }

        private SignUpResult RegisterAccount(IUnitOfWork unitOfWork, AccountForm form)
        {
            SignUpResult result;
            try
            {
                if (unitOfWork.Players.FindByEmail(form.Email.Trim().ToLowerInvariant()) != null)
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
                if (IsUniqueEmailViolation(ex))
                {
                    result = SignUpResult.Failure(EmailAlreadyRegisteredMessageKey);
                }
                else
                {
                    result = SignUpResult.Failure(UnexpectedErrorMessageKey);
                }
            }
            catch (ArgumentException)
            {
                // The entities reject any value that the checks above let through.
                // The attempt then ends with a message and an untouched database instead of a crash.
                unitOfWork.Rollback();
                result = SignUpResult.Failure(UnexpectedErrorMessageKey);
            }

            return result;
        }

        private SignUpResult SaveNewAccount(IUnitOfWork unitOfWork, AccountForm form)
        {
            Locale locale = unitOfWork.Locales.FindByCode(form.CultureCode)
                ?? unitOfWork.Locales.FindByCode(LocalizationProvider.DefaultCultureCode);

            Player newPlayer = new Player(
                form.Username,
                form.Email,
                _passwordHasher.HashPassword(form.Password),
                locale.LocaleId);
            unitOfWork.Players.Add(newPlayer);

            string plainCode = _codeGenerator.GenerateCode();
            VerificationCode verificationCode = new VerificationCode(
                newPlayer.PlayerId,
                VerificationPurpose.CreateAccount,
                _codeHasher.ComputeHash(plainCode),
                DateTime.UtcNow.AddMinutes(VerificationCodeLifetimeMinutes));
            unitOfWork.VerificationCodes.Add(verificationCode);

            unitOfWork.Commit();

            return SignUpResult.Success(plainCode);
        }

        private sealed class AccountForm
        {
            public string Email { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string CultureCode { get; set; }
        }
    }
}
