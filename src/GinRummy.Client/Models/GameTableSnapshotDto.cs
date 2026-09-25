using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    /// <summary>
    /// State of a match as the game table shows it to one of its players: the hand of the
    /// player, what can be seen of the opponent, the stock, the discard pile, the score, the
    /// log and the chat of the match.
    /// </summary>
    public sealed class GameTableSnapshotDto
    {
        /// <summary>
        /// Gets or sets the name of the player who looks at the table.
        /// </summary>
        public string PlayerName { get; set; }
        /// <summary>
        /// Gets or sets the name of the opponent.
        /// </summary>
        public string OpponentName { get; set; }
        /// <summary>
        /// Gets or sets how many cards the opponent holds, which are shown face down.
        /// </summary>
        public int OpponentCardCount { get; set; }
        /// <summary>
        /// Gets or sets how many cards are left in the stock.
        /// </summary>
        public int StockCount { get; set; }
        /// <summary>
        /// Gets or sets the card on top of the discard pile.
        /// </summary>
        public CardDto DiscardTop { get; set; }
        /// <summary>
        /// Gets or sets the cards of the player, in the order the player keeps them.
        /// </summary>
        public IList<CardDto> Hand { get; set; }
        /// <summary>
        /// Gets or sets the deadwood of the hand of the player.
        /// </summary>
        public int Deadwood { get; set; }
        /// <summary>
        /// Gets or sets the score of the player in the match.
        /// </summary>
        public int PlayerScore { get; set; }
        /// <summary>
        /// Gets or sets the score of the opponent in the match.
        /// </summary>
        public int OpponentScore { get; set; }
        /// <summary>
        /// Gets or sets the score that wins the match.
        /// </summary>
        public int TargetScore { get; set; }
        /// <summary>
        /// Gets or sets the messages of the chat of the match, from the oldest to the newest.
        /// </summary>
        public IList<ChatMessageDto> ChatEntries { get; set; }
        /// <summary>
        /// Gets or sets the events of the match, from the oldest to the newest.
        /// </summary>
        public IList<MatchLogEntryDto> MatchLog { get; set; }
    }
}
