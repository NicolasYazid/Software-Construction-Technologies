namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// Stage of a match. The names match the values the game_state column of the GameSession
    /// table accepts.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// The match exists but has not started, because it is waiting for its players.
        /// </summary>
        Waiting,
        /// <summary>
        /// The match is being played.
        /// </summary>
        InProgress,
        /// <summary>
        /// The match is stopped while a disconnected player tries to come back.
        /// </summary>
        Paused,
        /// <summary>
        /// The match is over.
        /// </summary>
        Finished
    }
}
