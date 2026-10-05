namespace GinRummy.Client.Models
{
    // External account the player may link to the profile, as the profile panel lists it.
    public sealed class LinkedAccountDto
    {
        public string PlatformName { get; set; }
        public bool IsLinked { get; set; }
    }
}
