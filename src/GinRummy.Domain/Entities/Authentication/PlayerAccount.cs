using System;
using System.Collections.Generic;
using System.Net.Mail;

using GinRummy.Domain.Entities.PlayerCareer;
using GinRummy.Domain.Entities.PlayerCustomization;
using GinRummy.Domain.Shared;

namespace GinRummy.Domain.Entities.Authentication
{
    public class PlayerAccount
    {
        public const int EmailMaxLength = 254;

        public const int PasswordHashMaxLength = 255;

        public PlayerAccount(string email, string passwordHash, DateTime createdAt)
        {
            ValidateEmail(email);
            ValidatePasswordHash(passwordHash);

            Email = NormalizeEmail(email);
            PasswordHash = passwordHash;
            CreatedAt = createdAt;
            IsEmailVerified = false;
            IsTwoFactorEnabled = false;
        }

        protected PlayerAccount()
        {
        }

        public int PlayerAccountId { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public bool IsEmailVerified { get; private set; }
        public bool IsTwoFactorEnabled { get; private set; }
        public virtual Entities.PlayerPreferences.PlayerPreferences PlayerPreferences { get; private set; }
        public virtual PlayerProfile PlayerProfile { get; private set; }
        public virtual PlayerStats PlayerStats { get; private set; }

        public static IList<EmailRequirement> GetUnmetEmailRequirements(string email)
        {
            string text = email ?? string.Empty;
            List<EmailRequirement> unmetRequirements = new List<EmailRequirement>();

            if (string.IsNullOrWhiteSpace(text))
            {
                unmetRequirements.Add(EmailRequirement.Required);
            }
            else if (!EmailHasValidFormat(text.Trim()))
            {
                unmetRequirements.Add(EmailRequirement.ValidFormat);
            }

            if (text.Trim().Length > EmailMaxLength)
            {
                unmetRequirements.Add(EmailRequirement.MaximumLength);
            }

            if (ForbiddenCharacterCommon.ContainsForbiddenCharacters(text))
            {
                unmetRequirements.Add(EmailRequirement.NoForbiddenCharacters);
            }

            return unmetRequirements;
        }

        public void MarkEmailVerified()
        {
            IsEmailVerified = true;
        }

        public void RegisterLogin(DateTime loggedInAt)
        {
            LastLoginAt = loggedInAt;
        }

        public void ChangePasswordHash(string passwordHash)
        {
            ValidatePasswordHash(passwordHash);

            PasswordHash = passwordHash;
        }

        public void ChangeEmail(string email)
        {
            ValidateEmail(email);

            Email = NormalizeEmail(email);
        }

        public void EnableTwoFactor()
        {
            IsTwoFactorEnabled = true;
        }

        public void DisableTwoFactor()
        {
            IsTwoFactorEnabled = false;
        }

        public static string NormalizeEmail(string email)
        {
            string normalizedEmail = email.Trim().ToLowerInvariant();

            return normalizedEmail;
        }

        private static void ValidateEmail(string email)
        {
            IList<EmailRequirement> unmetRequirements = GetUnmetEmailRequirements(email);

            if (unmetRequirements.Count > 0)
            {
                throw new ArgumentException(
                    "The email does not meet these requirements: " + string.Join(", ", unmetRequirements) + ".",
                    nameof(email));
            }
        }

        private static bool EmailHasValidFormat(string email)
        {
            bool isValid;

            try
            {
                MailAddress parsedAddress = new MailAddress(email);
                isValid = string.Equals(parsedAddress.Address, email, StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                isValid = false;
            }

            return isValid;
        }

        private static void ValidatePasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Password hash is required.", nameof(passwordHash));
            }
        }
    }
}
