namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// How much of the game a ban restricts. The names match the values the restriction_scope
    /// column of the ReasonBan table accepts.
    /// </summary>
    public enum RestrictionScope
    {
        /// <summary>
        /// The player cannot write in any chat but can still sign in and play.
        /// </summary>
        ChatOnly,
        /// <summary>
        /// The player cannot sign in or play.
        /// </summary>
        FullAccount
    }
}
