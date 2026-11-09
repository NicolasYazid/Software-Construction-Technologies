using System;

namespace GinRummy.Domain.Entities.PlayerCustomization
{
    /// <summary>
    /// A link to a social network shown on a player's profile, mapped to the SocialMedia
    /// table. The player writes both the name of the network and the address. Protects its
    /// own invariants: it always belongs to a profile and its name and url fit their columns.
    /// </summary>
    public class SocialMedia
    {
        /// <summary>
        /// Length of the url column.
        /// </summary>
        public const int UrlMaxLength = 255;

        /// <summary>
        /// Length of the platform name column.
        /// </summary>
        public const int PlatformNameMaxLength = 100;

        /// <summary>
        /// Creates a valid new social media link.
        /// </summary>
        /// <param name="playerAccountId">Identifier of the profile that shows the link; must be positive.</param>
        /// <param name="platformName">Name of the network as the player wrote it; required, at most 100 characters.</param>
        /// <param name="url">Address of the player's page; required, at most 255 characters.</param>
        public SocialMedia(int playerAccountId, string platformName, string url)
        {
            ValidatePlayerAccountId(playerAccountId);
            ValidateText(platformName, PlatformNameMaxLength, nameof(platformName));
            ValidateText(url, UrlMaxLength, nameof(url));

            PlayerAccountId = playerAccountId;
            PlatformName = platformName.Trim();
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
        public int PlayerAccountId { get; private set; }

        /// <summary>
        /// Gets the name of the network as the player wrote it.
        /// </summary>
        public string PlatformName { get; private set; }

        /// <summary>
        /// Gets the address of the player's page on the platform.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Tells whether an address is the address of a web page.
        /// </summary>
        /// <param name="url">Address to check.</param>
        /// <returns>True when the address is absolute and uses http or https.</returns>
        public static bool IsWebAddress(string url)
        {
            Uri address;
            bool isAbsolute = Uri.TryCreate(url, UriKind.Absolute, out address);
            bool isWebAddress = isAbsolute
                && ((address.Scheme == Uri.UriSchemeHttp) || (address.Scheme == Uri.UriSchemeHttps));

            return isWebAddress;
        }

        // The link belongs to an account that already exists, so its id must be positive.
        private static void ValidatePlayerAccountId(int playerAccountId)
        {
            if (playerAccountId <= 0)
            {
                throw new ArgumentException("Player account id must be positive.", nameof(playerAccountId));
            }
        }

        // The name and the url must fit their columns.
        // Their content is checked as input validation before the link is built.
        private static void ValidateText(string text, int maxLength, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Text is required.", parameterName);
            }

            if (text.Trim().Length > maxLength)
            {
                throw new ArgumentException("Text is too long.", parameterName);
            }
        }
    }
}
