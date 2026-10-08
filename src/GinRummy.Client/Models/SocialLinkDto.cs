namespace GinRummy.Client.Models
{
    // The name of the platform comes from its catalogue and the address is written by the player, so neither is translated.
    public sealed class SocialLinkDto
    {
        public string PlatformName { get; set; }
        public string Url { get; set; }
    }
}
