using System;

namespace GinRummy.Domain.Entities.PlayerCustomization
{
    public class SocialMedia
    {
        public const int UrlMaxLength = 255;

        public const int PlatformNameMaxLength = 100;

        public SocialMedia(int playerAccountId, string platformName, string url)
        {
            ValidatePlayerAccountId(playerAccountId);
            ValidateText(platformName, PlatformNameMaxLength, nameof(platformName));
            ValidateText(url, UrlMaxLength, nameof(url));

            PlayerAccountId = playerAccountId;
            PlatformName = platformName.Trim();
            Url = url.Trim();
        }

        protected SocialMedia()
        {
        }

        public int SocialMediaId { get; private set; }

        public int PlayerAccountId { get; private set; }

        public string PlatformName { get; private set; }

        public string Url { get; private set; }

        public static bool IsWebAddress(string url)
        {
            Uri address;
            bool isAbsolute = Uri.TryCreate(url, UriKind.Absolute, out address);
            bool isWebAddress = isAbsolute
                && ((address.Scheme == Uri.UriSchemeHttp) || (address.Scheme == Uri.UriSchemeHttps));

            return isWebAddress;
        }

        private static void ValidatePlayerAccountId(int playerAccountId)
        {
            if (playerAccountId <= 0)
            {
                throw new ArgumentException("Player account id must be positive.", nameof(playerAccountId));
            }
        }

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
