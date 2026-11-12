using System;
using System.Collections.Generic;

using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Entities.PlayerPreferences;
using GinRummy.Domain.Ports.Authentication;
using GinRummy.Domain.Ports.PlayerPreferences;
using GinRummy.Domain.Shared;

namespace GinRummy.Domain.Entities.Authentication
{
    public class AccountRegistrar
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IClock _clock;

        public AccountRegistrar(IPasswordHasher passwordHasher, IClock clock)
        {
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

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

            unitOfWork.PlayerPreferences.Add(new Entities.PlayerPreferences.PlayerPreferences(newAccount.PlayerAccountId, locale.LocaleId));
            unitOfWork.PlayerAccounts.AddProfile(new PlayerProfile(newAccount.PlayerAccountId, form.Username));
            unitOfWork.PlayerAccounts.AddStats(new PlayerStats(newAccount.PlayerAccountId));

            return unitOfWork.PlayerAccounts.FindByPlayerAccountId(newAccount.PlayerAccountId);
        }

        private static void EnsureFormIsValid(SignUpForm form)
        {
            TypedFieldValidator.EnsureAreTyped(form.Email, form.Username, form.Password);
            TypedFieldValidator.EnsureHaveNoForbiddenCharacters(form.Email, form.Username, form.Password);
            EnsureEmailIsValid(form.Email);
            EnsureUsernameIsValid(form.Username);
            EnsurePasswordIsStrong(form.Password);
        }

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
