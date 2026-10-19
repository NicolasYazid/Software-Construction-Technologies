using System;
using System.ServiceModel;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// Events the server sends to each player of a match. Every method is one-way, and each event is built for
    /// the player who receives it, so the opponent's hand never travels while the hand is in play. The events
    /// depend on their order (a turn change after the card drawn), so the client implementation keeps the default
    /// ConcurrencyMode.Single: with Multiple, a probe received OnHandStarted after later events.
    /// </summary>
    public interface IGameCallback
    {
        /// <summary>
        /// A new hand was dealt; the upcard is the top of the discard pile.
        /// </summary>
        /// <param name="table">The table as this player sees it.</param>
        [OperationContract(IsOneWay = true)]
        void OnHandStarted(GameTableDto table);

        /// <summary>
        /// The card this player drew from the stock, which only they can see.
        /// </summary>
        /// <param name="card">The card drawn.</param>
        [OperationContract(IsOneWay = true)]
        void OnCardDrawn(CardDto card);

        /// <summary>
        /// The opponent made a move.
        /// </summary>
        /// <param name="move">The move, with the card only when it is visible.</param>
        [OperationContract(IsOneWay = true)]
        void OnOpponentMoved(OpponentMoveDto move);

        /// <summary>
        /// A move was accepted and the table changed; it reaches both players.
        /// </summary>
        /// <param name="turn">Whose turn it is and what comes next.</param>
        [OperationContract(IsOneWay = true)]
        void OnTurnChanged(TurnDto turn);

        /// <summary>
        /// The last move of this player was not accepted.
        /// </summary>
        /// <param name="rejection">The reason, as a code the client translates.</param>
        [OperationContract(IsOneWay = true)]
        void OnMoveRejected(MoveRejectedDto rejection);

        /// <summary>
        /// The hand ended by a knock, a gin, an undercut or a void stock.
        /// </summary>
        /// <param name="result">The result of the hand and the scores after it.</param>
        [OperationContract(IsOneWay = true)]
        void OnHandEnded(HandResultDto result);

        /// <summary>
        /// The match ended; after it, the client closes the channel of the table.
        /// </summary>
        /// <param name="result">The final result from this player's point of view.</param>
        [OperationContract(IsOneWay = true)]
        void OnMatchEnded(MatchResultDto result);

        /// <summary>
        /// The opponent lost the connection and the match is paused (DEC-09).
        /// </summary>
        /// <param name="reconnectDeadline">Moment, in UTC, when the match ends as an abandonment if the
        /// opponent has not come back.</param>
        [OperationContract(IsOneWay = true)]
        void OnOpponentDisconnected(DateTime reconnectDeadline);

        /// <summary>
        /// The opponent came back and the match resumed (DEC-09).
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void OnOpponentReconnected();
    }
}