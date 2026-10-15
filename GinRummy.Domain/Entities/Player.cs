using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A player as the game knows them, mapped to the Player table: the name, the public
    /// tag and the locale. The credentials of a registered player live in its
    /// PlayerAccount. Protects its own invariants: a player cannot exist without a valid
    /// username and locale.
    /// </summary>
    public class Player
    {
        private const int MaxUsernameLength = 20;

        /// <summary>
        /// Creates a valid new player, stamped with the current UTC time.
        /// </summary>
        /// <param name="username">Display name; required, at most 20 characters.</param>
        /// <param name="localeId">Locale chosen by the player; must be positive.</param>
        public Player(string username, int localeId)
        {
            ValidateUsername(username);
            ValidateLocaleId(localeId);

            Username = username.Trim();
            LocaleId = localeId;
            CreatedAt = DateTime.UtcNow;
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
        /// Gets the display name chosen at registration.
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

        // A player must always have a non-empty username within the length the database
        // column allows.
        private static void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is required.", nameof(username));
            }

            if (username.Trim().Length > MaxUsernameLength)
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
