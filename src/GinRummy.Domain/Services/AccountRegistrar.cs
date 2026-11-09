using System;
using System.Collections.Generic;

using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Entities.PlayerPreferences;
using GinRummy.Domain.Entities.Players;
using GinRummy.Domain.Ports.Infrastructure;
using GinRummy.Domain.Ports.PlayerPreferences;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Ports.Security;
using GinRummy.Domain.Security;

namespace GinRummy.Domain.Services
{
    /// <summary>
    /// Creates a new account with everything that comes with it: the account with its hashed
    /// password, the preferences, the profile and the initial statistics (CU-01 steps 4 to 8).
    /// </summary>
    public class AccountRegistrar
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IClock _clock;

        /// <summary>
        /// Builds the registrar with the password hasher and the clock the composition root assembled.
        /// </summary>
        /// <param name="passwordHasher">Hashes the password before it is stored.</param>
        /// <param name="clock">Tells the moment the account is created.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public AccountRegistrar(IPasswordHasher passwordHasher, IClock clock)
        {
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Checks what was typed and creates the account inside the transaction of the caller.
        /// </summary>
        /// <param name="unitOfWork">Unit of work of the sign-up; the caller commits it.</param>
        /// <param name="form">What the person typed to create the account.</param>
        /// <param name="localeCode">Culture code of the language the client was showing, such as es-MX.</param>
        /// <returns>The created account, with its preferences loaded.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the form is null.</exception>
        /// <exception cref="RequiredFieldMissingException">Thrown when a field is empty.</exception>
        /// <exception cref="DisallowedCharactersException">Thrown when a field contains forbidden characters.</exception>
        /// <exception cref="InvalidEmailFormatException">Thrown when the email is not a valid address.</exception>
        /// <exception cref="FieldTooLongException">Thrown when the username is longer than its column.</exception>
        /// <exception cref="WeakPasswordException">Thrown when the password does not meet the policy.</exception>
        /// <exception cref="EmailAlreadyRegisteredException">Thrown when another account already has the email.</exception>
        public PlayerAccount Register(IUnitOfWork unitOfWork, SignUpForm form, string localeCode)
        {
            if (form == null)
            {
                throw new ArgumentNullException(nameof(form));
            }

            EnsureFormIsValid(form);
            string email = PlayerAccount.NormalizeEmail(form.Email);
            if (unitOfWork.PlayerAccounts.FindByEmail(email) != null)
            {
                throw new EmailAlreadyRegisteredException("The email already belongs to another account.");
            }

            Locale locale = ResolveLocale(unitOfWork.Locales, localeCode);

            string passwordHash = _passwordHasher.HashPassword(new Password(form.Password));
            PlayerAccount newAccount = new PlayerAccount(email, passwordHash, _clock.UtcNow);
            unitOfWork.PlayerAccounts.Add(newAccount);

            unitOfWork.Players.Add(new PlayerPreferences(newAccount.PlayerAccountId, locale.LocaleId));
            unitOfWork.PlayerAccounts.AddProfile(new PlayerProfile(newAccount.PlayerAccountId, form.Username));
            unitOfWork.PlayerAccounts.AddStats(new PlayerStats(newAccount.PlayerAccountId));

            // The account is read again so it arrives with its preferences, whose language the email of the code is written in.
            return unitOfWork.PlayerAccounts.FindByPlayerAccountId(newAccount.PlayerAccountId);
        }

        // The checks run before anything is created, so a rejected sign-up leaves no data behind (CU-01 FA-01 to FA-03, EX-03).
        private static void EnsureFormIsValid(SignUpForm form)
        {
            TypedFieldValidator.EnsureAreTyped(form.Email, form.Username, form.Password);
            TypedFieldValidator.EnsureHaveNoForbiddenCharacters(form.Email, form.Username, form.Password);
            EnsureEmailIsValid(form.Email);
            EnsureUsernameIsValid(form.Username);
            EnsurePasswordIsStrong(form.Password);
        }

        // An address longer than its column is not a valid address either, so both cases share the format message.
        private static void EnsureEmailIsValid(string email)
        {
            IList<EmailRequirement> unmetRequirements = PlayerAccount.GetUnmetEmailRequirements(email);
            if (unmetRequirements.Contains(EmailRequirement.Required))
            {
                throw new RequiredFieldMissingException("The email has only blank characters.");
            }

            if (unmetRequirements.Count > 0)
            {
                throw new InvalidEmailFormatException("The email is not a valid address.");
            }
        }

        private static void EnsureUsernameIsValid(string username)
        {
            IList<UsernameRequirement> unmetRequirements = PlayerProfile.GetUnmetUsernameRequirements(username);
            if (unmetRequirements.Contains(UsernameRequirement.Required))
            {
                throw new RequiredFieldMissingException("The username has only blank characters.");
            }

            if (unmetRequirements.Contains(UsernameRequirement.MaximumLength))
            {
                throw new FieldTooLongException("The username is longer than its column.");
            }
        }

        private static void EnsurePasswordIsStrong(string password)
        {
            if (Password.GetUnmetRequirements(password).Count > 0)
            {
                throw new WeakPasswordException("The password does not meet the policy.");
            }
        }

        // A language the catalog does not have takes the default locale of the system (CU-01 EX-02).
        // Without a default locale the catalog itself is broken, which no player can fix, so it is an error of the server.
        private static Locale ResolveLocale(ILocaleRepository locales, string localeCode)
        {
            Locale locale = null;
            if (!string.IsNullOrWhiteSpace(localeCode))
            {
                locale = locales.FindByCode(localeCode.Trim());
            }

            return locale
                ?? locales.FindDefault()
                ?? throw new InvalidOperationException("No locale is marked as the default of the system.");
        }
    }
}
