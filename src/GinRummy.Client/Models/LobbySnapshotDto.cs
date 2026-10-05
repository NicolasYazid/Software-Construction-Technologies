using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    // State of the lobby at the moment a player enters it: the chat so far and the players of
    // the panel, already grouped the way the screen shows them. The counters of the groups of
    // players in a match are sent apart because each row of those groups holds two players.
    public sealed class LobbySnapshotDto
    {
        public string PlayerName { get; set; }
        public IList<ChatEntryDto> ChatEntries { get; set; }
        public IList<LobbyPlayerDto> LookingToPlay { get; set; }
        public IList<LobbyPlayerDto> Online { get; set; }
        public IList<MatchPairDto> MatchesInProgress { get; set; }
        public int PlayersInMatchCount { get; set; }
        public IList<LobbyPlayerDto> Unavailable { get; set; }
        public IList<LobbyPlayerDto> FriendsOnline { get; set; }
        public IList<MatchPairDto> FriendsInMatch { get; set; }
        public int FriendsInMatchCount { get; set; }
        public IList<LobbyPlayerDto> FriendsUnavailable { get; set; }
    }
}
