namespace GinRummy.Contracts.DataContracts
{
    /// <summary>
    /// Chat where the client shows a message it receives.
    /// </summary>
    public enum MessageDestination
    {
        /// <summary>
        /// The chat of the lobby, which every subscribed player sees.
        /// </summary>
        Lobby,

        /// <summary>
        /// The chat of the game table, which only the two players of the match see.
        /// </summary>
        Match
    }
}
