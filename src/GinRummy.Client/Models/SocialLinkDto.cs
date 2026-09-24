namespace GinRummy.Client.Models
{
    /// <summary>
    /// Link of a profile to a social network (CU-31). The name of the platform comes from its
    /// catalogue and the address is written by the player, so neither is translated.
    /// </summary>
    public sealed class SocialLinkDto
    {
        /// <summary>
        /// Gets or sets the name of the platform, as the catalogue of platforms delivers it.
        /// </summary>
        public string PlatformName { get; set; }
        /// <summary>
        /// Gets or sets the address the player wrote for that platform.
        /// </summary>
        public string Url { get; set; }
    }
}
