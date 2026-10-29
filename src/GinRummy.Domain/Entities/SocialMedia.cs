using System;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A link to a social network shown on a player's profile, mapped to the SocialMedia
    /// table. Protects its own invariants: it always belongs to a profile and a platform and
    /// always has a url that fits the column.
    /// </summary>
    public class SocialMedia
    {
        /// <summary>
        /// Length of the url column.
        /// </summary>
        public const int UrlMaxLength = 255;

        /// <summary>
        /// Creates a valid new social media link.
        /// </summary>
        /// <param name="playerId">Identifier of the profile that shows the link; must be positive.</param>
        /// <param name="platformId">Identifier of the platform the link points to; must be positive.</param>
        /// <param name="url">Address of the player's page; required, at most 255 characters.</param>
        public SocialMedia(int playerId, int platformId, string url)
        {
            ValidateId(playerId, nameof(playerId));
            ValidateId(platformId, nameof(platformId));
            ValidateUrl(url);

            PlayerId = playerId;
            PlatformId = platformId;
            Url = url.Trim();
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected SocialMedia()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int SocialMediaId { get; private set; }

        /// <summary>
        /// Gets the identifier of the player whose profile shows the link.
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        /// Gets the identifier of the platform the link points to.
        /// </summary>
        public int PlatformId { get; private set; }

        /// <summary>
        /// Gets the address of the player's page on the platform.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Gets the platform the link points to, loaded from the relationship between the
        /// two tables.
        /// </summary>
        public virtual Platform Platform { get; private set; }

        // The link references rows that already exist, so their ids must be positive.
        private static void ValidateId(int id, string parameterName)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be positive.", parameterName);
            }
        }

        // The url must fit the column.
        // Its structure is checked as input validation before the link is built.
        private static void ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Url is required.", nameof(url));
            }

            if (url.Trim().Length > UrlMaxLength)
            {
                throw new ArgumentException("Url is too long.", nameof(url));
            }
        }
    }
}
