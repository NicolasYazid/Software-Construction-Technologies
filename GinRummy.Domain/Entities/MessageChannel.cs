namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// Where a chat message was written. The names match the values the channel column of the
    /// Message table accepts.
    /// </summary>
    public enum MessageChannel
    {
        /// <summary>
        /// The message was written in the lobby chat and belongs to no match.
        /// </summary>
        Lobby,
        /// <summary>
        /// The message was written during a match and belongs to its game session.
        /// </summary>
        Match
    }
}
