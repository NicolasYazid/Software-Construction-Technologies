namespace GinRummy.Client.Models
{
    /// <summary>
    /// External account the player may link to the profile, as the profile panel lists it.
    /// </summary>
    public sealed class LinkedAccountDto
    {
        /// <summary>
        /// Gets or sets the name of the platform of the account.
        /// </summary>
        public string PlatformName { get; set; }
        /// <summary>
        /// Gets or sets whether the account is linked, which decides the action of its row.
        /// </summary>
        public bool IsLinked { get; set; }
    }
}
