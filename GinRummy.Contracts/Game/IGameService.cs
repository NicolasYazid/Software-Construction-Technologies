using System.ServiceModel;

using GinRummy.Contracts.Common;

namespace GinRummy.Contracts.Game
{
    /// <summary>
    /// Network contract of the match table (S13). It keeps its own duplex channel, apart from the notifications
    /// one, because its events only matter to the two players of the match and happen several times per turn
    /// (DEC-04). The player is identified by the token once, when the session starts, and then by the channel.
    /// Moves are one-way: their result arrives through <see cref="IGameCallback"/>.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IGameCallback))]
    public interface IGameService
    {
        /// <summary>
        /// Sits the player at the table and registers the callback channel of the match (CU-22 step 11, CU-23
        /// step 12). The cards are dealt once both players have joined, and arrive through OnHandStarted.
        /// </summary>
        /// <param name="request">The session token and the match the MatchFound notification announced.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown with SessionExpired when the token is not valid, NotFound when the player is not part of the
        /// match, or Conflict when the match already finished.
        /// </exception>
        [OperationContract(IsInitiating = true)]
        [FaultContract(typeof(ServiceFault))]
        void JoinTable(JoinTableRequest request);

        /// <summary>
        /// Brings a player back to the open match they lost the connection to, registers the new channel and
        /// resumes the match (DEC-09).
        /// </summary>
        /// <param name="request">The session token of the player who comes back.</param>
        /// <returns>The table as the player left it, with their hand.</returns>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown with SessionExpired when the token is not valid, or NotFound when the player has no open match.
        /// </exception>
        [OperationContract(IsInitiating = true)]
        [FaultContract(typeof(ServiceFault))]
        GameTableDto RejoinMatch(SessionRequest request);

        /// <summary>
        /// Takes the card turned up after the deal (house rule 2).
        /// </summary>
        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void TakeUpcard();

        /// <summary>
        /// Passes on the card turned up after the deal; if both players pass, the non-dealer draws from the stock
        /// (house rule 2).
        /// </summary>
        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void PassUpcard();

        /// <summary>
        /// Draws the top card of the stock (house rule 3). The card arrives only to this player, through
        /// OnCardDrawn.
        /// </summary>
        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void DrawFromStock();

        /// <summary>
        /// Draws the top card of the discard pile (house rule 3).
        /// </summary>
        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void DrawFromDiscardPile();

        /// <summary>
        /// Discards a card and ends the turn (house rules 3, 4 and 16).
        /// </summary>
        /// <param name="card">The card to discard; it must be in the hand and cannot be the one just taken from
        /// the discard pile.</param>
        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void Discard(CardDto card);

        /// <summary>
        /// Discards a card and closes the hand. The server arranges the melds for the least deadwood, decides
        /// whether it is a knock or a gin, and lays off the defender's cards (house rules 8 to 11).
        /// </summary>
        /// <param name="discardedCard">The card discarded to close; the deadwood left must be at most the knock
        /// limit.</param>
        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void CloseHand(CardDto discardedCard);

        /// <summary>
        /// Confirms the result of the hand; the next hand is dealt when both players confirm (house rules 14
        /// and 15).
        /// </summary>
        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void ConfirmNextHand();

        /// <summary>
        /// Abandons the match, which counts as a defeat, and ends the session of the table (CU-26, house rule 19).
        /// </summary>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown with Conflict when the match already finished.
        /// </exception>
        [OperationContract(IsInitiating = false, IsTerminating = true)]
        [FaultContract(typeof(ServiceFault))]
        void ForfeitMatch();
    }
}