using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    /// <summary>
    /// State of the lobby at the moment a player enters it: the chat so far and the players of
    /// the panel, already grouped the way the screen shows them. The counters of the groups of
    /// players in a match are sent apart because each row of those groups holds two players.
    /// </summary>
    public sealed class LobbySnapshotDto
    {
        /// <summary>
        /// Gets or sets the name of the player who enters, or the temporary name of a guest.
        /// </summary>
        public string PlayerName { get; set; }
        /// <summary>
        /// Gets or sets the messages and notices of the chat, from the oldest to the newest.
        /// </summary>
        public IList<ChatEntryDto> ChatEntries { get; set; }
        /// <summary>
        /// Gets or sets the players who are looking for a match.
        /// </summary>
        public IList<LobbyPlayerDto> LookingToPlay { get; set; }
        /// <summary>
        /// Gets or sets the players who are connected and free, as the guest lobby groups them.
        /// </summary>
        public IList<LobbyPlayerDto> Online { get; set; }
        /// <summary>
        /// Gets or sets the matches in progress between the players of the lobby.
        /// </summary>
        public IList<MatchPairDto> MatchesInProgress { get; set; }
        /// <summary>
        /// Gets or sets how many players are in a match.
        /// </summary>
        public int PlayersInMatchCount { get; set; }
        /// <summary>
        /// Gets or sets the players who are not available.
        /// </summary>
        public IList<LobbyPlayerDto> Unavailable { get; set; }
        /// <summary>
        /// Gets or sets the friends who are connected and free.
        /// </summary>
        public IList<LobbyPlayerDto> FriendsOnline { get; set; }
        /// <summary>
        /// Gets or sets the matches in progress in which a friend plays.
        /// </summary>
        public IList<MatchPairDto> FriendsInMatch { get; set; }
        /// <summary>
        /// Gets or sets how many friends are in a match.
        /// </summary>
        public int FriendsInMatchCount { get; set; }
        /// <summary>
        /// Gets or sets the friends who are not available.
        /// </summary>
        public IList<LobbyPlayerDto> FriendsUnavailable { get; set; }
    }
}
