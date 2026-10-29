using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A player as the game knows them, mapped to the Player table: the name, the public
    /// tag, the locale and the chat filter preference. The credentials of a registered
    /// player live in its PlayerAccount. Protects its own invariants: a player cannot exist
    /// without a valid username and locale.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Length of the username column.
        /// </summary>
        public const int UsernameMaxLength = 20;

        /// <summary>
        /// Creates a valid new player registered at the given moment. The chat filter starts
        /// enabled.
        /// </summary>
        /// <param name="username">Display name; required, at most 20 characters.</param>
        /// <param name="localeId">Locale chosen by the player; must be positive.</param>
        /// <param name="createdAt">Moment of registration, in UTC.</param>
        public Player(string username, int localeId, DateTime createdAt)
        {
            ValidateUsername(username);
            ValidateLocaleId(localeId);

            Username = username.Trim();
            LocaleId = localeId;
            CreatedAt = createdAt;
            IsChatFilterEnabled = true;
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected Player()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        /// Gets the public identifier shown to other players, always PlayerId + 100000.
        /// Computed by SQL Server; never assigned by the application.
        /// </summary>
        public int PublicTag { get; private set; }

        /// <summary>
        /// Gets the display name chosen by the player. It is not unique; the public tag tells
        /// players with the same name apart.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets the locale configured for the player.
        /// </summary>
        public int LocaleId { get; private set; }

        /// <summary>
        /// Gets the registration date, in UTC.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets whether the player wants prohibited words hidden in the messages they read.
        /// </summary>
        public bool IsChatFilterEnabled { get; private set; }

        /// <summary>
        /// Replaces the display name.
        /// </summary>
        /// <param name="username">New display name; required, at most 20 characters.</param>
        public void ChangeUsername(string username)
        {
            ValidateUsername(username);

            Username = username.Trim();
        }

        /// <summary>
        /// Replaces the locale configured for the player.
        /// </summary>
        /// <param name="localeId">Identifier of the new locale; must be positive.</param>
        public void ChangeLocale(int localeId)
        {
            ValidateLocaleId(localeId);

            LocaleId = localeId;
        }

        // A player must always have a non-empty username within the length the database
        // column allows.
        private static void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is required.", nameof(username));
            }

            if (username.Trim().Length > UsernameMaxLength)
            {
                throw new ArgumentException("Username is too long.", nameof(username));
            }
        }

        // The locale is a foreign key, so it must reference a real, positive id.
        private static void ValidateLocaleId(int localeId)
        {
            if (localeId <= 0)
            {
                throw new ArgumentException("Locale id must be positive.", nameof(localeId));
            }
        }
    }
}
