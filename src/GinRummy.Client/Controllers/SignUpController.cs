using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Client.Localization;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;
using GinRummy.Domain.Security;

namespace GinRummy.Client.Controllers
{
    /// <summary>
    /// Orchestrates CU-01 (Create an account): validates the form, then creates the
    /// player and its verification code together, so both succeed or neither does.
    /// </summary>
    public class SignUpController
    {
        private const string RequiredFieldMessageKey = "Error_ValRequiredField";
        private const string InvalidEmailMessageKey = "Error_ValInvalidEmailFormat";
        private const string WeakPasswordMessageKey = "Error_ValWeakPassword";
        private const string EmailAlreadyRegisteredMessageKey = "Error_AuthEmailAlreadyRegistered";
        private const string UnexpectedErrorMessageKey = "Error_SysUnexpected";
        private const int MinimumPasswordLength = 8;
        private const int VerificationCodeLifetimeMinutes = 10;
        private const int UniqueConstraintViolationErrorNumber = 2627;
        private const int UniqueIndexViolationErrorNumber = 2601;

        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IVerificationCodeGenerator _codeGenerator;
        private readonly IVerificationCodeHasher _codeHasher;

        /// <summary>
        /// Builds the controller with a way to open a unit of work and the security
        /// adapters the composition root already assembled.
        /// </summary>
        /// <param name="unitOfWorkFactory">Provides a fresh unit of work per operation.</param>
        /// <param name="passwordHasher">Adapter that hashes and checks passwords.</param>
        /// <param name="codeGenerator">Adapter that generates verification codes.</param>
        /// <param name="codeHasher">Adapter that hashes and checks verification codes.</param>
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

        /// <summary>
        /// Creates a new account, or reports which validation failed.
        /// </summary>
        /// <param name="email">Email address written on the form.</param>
        /// <param name="username">Username written on the form.</param>
        /// <param name="password">Password written on the form.</param>
        /// <param name="activeCultureCode">Culture code active on the client right now.</param>
        /// <returns>The outcome of the attempt.</returns>
        public SignUpResult CreateAccount(string email, string username, string password, string activeCultureCode)
        {
            SignUpResult result = null;

            if (AreAnyFieldsEmpty(email, username, password))
            {
                result = SignUpResult.Failure(RequiredFieldMessageKey);
            }

            if (result == null && !IsValidEmailFormat(email))
            {
                result = SignUpResult.Failure(InvalidEmailMessageKey);
            }

            if (result == null && !IsStrongPassword(password))
            {
                result = SignUpResult.Failure(WeakPasswordMessageKey);
            }

            if (result == null)
            {
                result = CreateAccountRecord(email, username, password, activeCultureCode);
            }

            return result;
        }

        // FA-01: none of the three fields may be empty.
        private static bool AreAnyFieldsEmpty(string email, string username, string password)
        {
            return string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(username)
                || string.IsNullOrWhiteSpace(password);
        }

        // FA-02: MailAddress throws FormatException for anything not shaped like an email
        // address, so it validates without a hand-written regular expression to maintain.
        private static bool IsValidEmailFormat(string email)
        {
            bool isValid;
            try
            {
                MailAddress parsedAddress = new MailAddress(email);
                isValid = true;
            }
            catch (FormatException)
            {
                isValid = false;
            }

            return isValid;
        }

        // FA-03: the draft policy already shown on GuiSignUp — at least 8 characters,
        // at least one letter, at least one digit.
        private static bool IsStrongPassword(string password)
        {
            bool hasMinimumLength = password.Length >= MinimumPasswordLength;
            bool hasLetter = password.Any(character => char.IsLetter(character));
            bool hasDigit = password.Any(character => char.IsDigit(character));

            return hasMinimumLength && hasLetter && hasDigit;
        }

        // Everything here happens through one unit of work: the player and its code are
        // both committed together, or neither is.
        private SignUpResult CreateAccountRecord(
            string email, string username, string password, string activeCultureCode)
        {
            SignUpResult result;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                try
                {
                    if (unitOfWork.Players.FindByEmail(email.Trim().ToLowerInvariant()) != null)
                    {
                        result = SignUpResult.Failure(EmailAlreadyRegisteredMessageKey);
                    }
                    else
                    {
                        result = SaveNewAccount(unitOfWork, email, username, password, activeCultureCode);
                    }
                }
                catch (DbUpdateException databaseUpdateException)
                {
                    unitOfWork.Rollback();
                    result = IsUniqueEmailViolation(databaseUpdateException)
                        ? SignUpResult.Failure(EmailAlreadyRegisteredMessageKey)
                        : SignUpResult.Failure(UnexpectedErrorMessageKey);
                }
            }

            return result;
        }

        // Creates the player and its verification code, then commits both at once.
        private SignUpResult SaveNewAccount(
            IUnitOfWork unitOfWork, string email, string username, string password, string activeCultureCode)
        {
            Locale locale = unitOfWork.Locales.FindByCode(activeCultureCode)
                ?? unitOfWork.Locales.FindByCode(LocalizationProvider.DefaultCultureCode);

            Player newPlayer = new Player(
                username,
                email,
                _passwordHasher.HashPassword(password),
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

        // EF6 wraps a SQL Server error in two layers: DbUpdateException -> InnerException
        // -> the real SqlException. Error 2627 is a UNIQUE KEY violation, 2601 a duplicate
        // key in a unique index — SQL Server may raise either depending on the constraint.
        private static bool IsUniqueEmailViolation(DbUpdateException databaseUpdateException)
        {
            bool isUniqueViolation = false;
            SqlException sqlException = databaseUpdateException.InnerException?.InnerException as SqlException;
            if (sqlException != null)
            {
                isUniqueViolation = sqlException.Number == UniqueConstraintViolationErrorNumber
                    || sqlException.Number == UniqueIndexViolationErrorNumber;
            }

            return isUniqueViolation;
        }
    }
}
