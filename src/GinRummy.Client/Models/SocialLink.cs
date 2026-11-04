using System;

namespace GinRummy.Client.Models
{
    // The name of the platform comes from its catalogue and the address is written by the player, so neither is translated.
    public sealed class SocialLink
    {
        public SocialLink(string platformName, string url)
        {
            PlatformName = platformName ?? throw new ArgumentNullException(nameof(platformName));
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        public string PlatformName { get; private set; }
        public string Url { get; private set; }
    }
}
