using System.Collections.Generic;

using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Models
{
    // The counters of the groups of players in a match are sent apart because each row of those groups holds two players.
    public sealed class LobbySnapshot
    {
        public string PlayerName { get; set; }
        public IList<ChatEntry> ChatEntries { get; set; }
        public IList<LobbyPlayerViewModel> LookingToPlay { get; set; }
        public IList<LobbyPlayerViewModel> Online { get; set; }
        public IList<MatchPair> MatchesInProgress { get; set; }
        public int PlayersInMatchCount { get; set; }
        public IList<LobbyPlayerViewModel> Unavailable { get; set; }
        public IList<LobbyPlayerViewModel> FriendsOnline { get; set; }
        public IList<MatchPair> FriendsInMatch { get; set; }
        public int FriendsInMatchCount { get; set; }
        public IList<LobbyPlayerViewModel> FriendsUnavailable { get; set; }
    }
}
