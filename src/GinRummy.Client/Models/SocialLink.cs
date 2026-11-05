using System;

namespace GinRummy.Client.Models
{
    // The player writes both the name of the network and the address, so neither is translated.
    public sealed class SocialLink
    {
        public SocialLink(int socialMediaId, string platformName, string url)
        {
            SocialMediaId = socialMediaId;
            PlatformName = platformName ?? throw new ArgumentNullException(nameof(platformName));
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        public int SocialMediaId { get; private set; }
        public string PlatformName { get; private set; }
        public string Url { get; private set; }
    }
}
