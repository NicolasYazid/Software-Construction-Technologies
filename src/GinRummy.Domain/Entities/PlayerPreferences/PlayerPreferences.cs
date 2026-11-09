using System;

namespace GinRummy.Domain.Entities.PlayerPreferences
{
    public class PlayerPreferences
    {
        public PlayerPreferences(int playerAccountId, int localeId)
        {
            ValidatePlayerAccountId(playerAccountId);
            ValidateLocaleId(localeId);

            PlayerAccountId = playerAccountId;
            LocaleId = localeId;
            IsChatFilterEnabled = true;
        }

        protected PlayerPreferences()
        {
        }

        public int PlayerAccountId { get; private set; }
        public int LocaleId { get; private set; }
        public bool IsChatFilterEnabled { get; private set; }

        /// <summary>
        /// Gets the language the player chose, loaded from the relationship between the two tables.
        /// </summary>
        public virtual Locale Locale { get; private set; }

        public void ChangeChatFilter(bool isChatFilterEnabled)
        {
            IsChatFilterEnabled = isChatFilterEnabled;
        }

        public void ChangeLocale(int localeId)
        {
            ValidateLocaleId(localeId);

            LocaleId = localeId;
        }

        private static void ValidatePlayerAccountId(int playerAccountId)
        {
            if (playerAccountId <= 0)
            {
                throw new ArgumentException("Player account id must be positive.", nameof(playerAccountId));
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