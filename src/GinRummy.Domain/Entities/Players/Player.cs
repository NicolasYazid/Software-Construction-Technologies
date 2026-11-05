using System;
using System.Collections.Generic;
using GinRummy.Domain.Services;

namespace GinRummy.Domain.Entities.Players
{
    public class Player
    {
        public const int UsernameMaxLength = 20;

        public Player(string username, int localeId, DateTime createdAt)
        {
            ValidateUsername(username);
            ValidateLocaleId(localeId);

            Username = username.Trim();
            LocaleId = localeId;
            CreatedAt = createdAt;
            IsChatFilterEnabled = true;
        }

        protected Player()
        {
        }

        public int PlayerId { get; private set; }
        public int PublicTag { get; private set; }
        public string Username { get; private set; }
        public int LocaleId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsChatFilterEnabled { get; private set; }

        /// <summary>
        /// Gets the language the player chose, loaded from the relationship between the two tables.
        /// </summary>
        public virtual Locale Locale { get; private set; }

        public static IList<UsernameRequirement> GetUnmetUsernameRequirements(string username)
        {
            string text;

            if (username != null)
            {
                text = username;
            }
            else
            {
                text = string.Empty;
            }

            List<UsernameRequirement> unmetRequirements = new List<UsernameRequirement>();

            if (string.IsNullOrWhiteSpace(text))
            {
                unmetRequirements.Add(UsernameRequirement.Required);
            }

            if (text.Trim().Length > UsernameMaxLength)
            {
                unmetRequirements.Add(UsernameRequirement.MaximumLength);
            }

            if (ForbiddenCharacterCommon.ContainsForbiddenCharacters(text))
            {
                unmetRequirements.Add(UsernameRequirement.NoForbiddenCharacters);
            }

            return unmetRequirements;
        }

        public void ChangeUsername(string username)
        {
            ValidateUsername(username);

            Username = username.Trim();
        }

        public void ChangeChatFilter(bool isChatFilterEnabled)
        {
            IsChatFilterEnabled = isChatFilterEnabled;
        }

        public void ChangeLocale(int localeId)
        {
            ValidateLocaleId(localeId);

            LocaleId = localeId;
        }

        private static void ValidateUsername(string username)
        {
            IList<UsernameRequirement> unmetRequirements = GetUnmetUsernameRequirements(username);

            if (unmetRequirements.Count > 0)
            {
                throw new ArgumentException(
                    "The username does not meet these requirements: " + string.Join(", ", unmetRequirements) + ".",
                    nameof(username));
            }
        }

        private static void ValidateLocaleId(int localeId)
        {
            if (localeId <= 0)
            {
                throw new ArgumentException("Locale id must be positive.", nameof(localeId));
            }
        }
    }
}
