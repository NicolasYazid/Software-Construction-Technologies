namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A social network a player can link to the profile, mapped to the Platform table. The
    /// game's code only ever reads this catalog; it never creates or edits a platform.
    /// </summary>
    public class Platform
    {
        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization.
        /// </summary>
        protected Platform()
        {
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int PlatformId { get; private set; }

        /// <summary>
        /// Gets the name of the social network, such as "Instagram".
        /// </summary>
        public string Name { get; private set; }
    }
}
