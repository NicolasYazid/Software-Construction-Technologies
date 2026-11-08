using System;

using GinRummy.Domain.Entities.Players;

namespace GinRummy.Domain.Entities.Game
{
    /// <summary>
    /// The seat of a player or a guest in a match, mapped to the GameParticipant table. The
    /// cards in hand are kept as the JSON text the database stores. Protects its own
    /// invariants: a seat belongs to exactly one registered player or to exactly one guest,
    /// and its score is never negative.
    /// </summary>
    public class GameParticipant
    {
        private const int InitialScore = 0;

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the factory methods.
        /// </summary>
        protected GameParticipant()
        {
        }

        private GameParticipant(int gameSessionId)
        {
            GameSessionId = gameSessionId;
            Score = InitialScore;
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int GameParticipantId { get; private set; }

        /// <summary>
        /// Gets the identifier of the match the seat belongs to.
        /// </summary>
        public int GameSessionId { get; private set; }

        /// <summary>
        /// Gets the identifier of the registered player. Null when a guest holds the seat.
        /// </summary>
        public int? PlayerId { get; private set; }

        /// <summary>
        /// Gets the identifier of the guest session. Null when a registered player holds the seat.
        /// </summary>
        public int? GuestSessionId { get; private set; }

        /// <summary>
        /// Gets the cards in hand, as JSON text. Null until the cards are dealt.
        /// </summary>
        public string CardsInHand { get; private set; }

        /// <summary>
        /// Gets the points of the participant in the match. The column keeps the name puntuation.
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// Gets the registered player that holds the seat. Null when a guest holds it.
        /// </summary>
        public virtual Player Player { get; private set; }

        /// <summary>
        /// Gets the guest that holds the seat. Null when a registered player holds it.
        /// </summary>
        public virtual GuestSession GuestSession { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a guest holds the seat.
        /// </summary>
        public bool IsGuest
        {
            get { return GuestSessionId.HasValue; }
        }

        /// <summary>
        /// Creates the seat of a registered player.
        /// </summary>
        /// <param name="gameSessionId">Identifier of the match; must be positive.</param>
        /// <param name="playerId">Identifier of the player; must be positive.</param>
        /// <returns>The new seat, with the initial score.</returns>
        public static GameParticipant ForPlayer(int gameSessionId, int playerId)
        {
            ValidateIdentifier(gameSessionId, nameof(gameSessionId));
            ValidateIdentifier(playerId, nameof(playerId));

            GameParticipant participant = new GameParticipant(gameSessionId);
            participant.PlayerId = playerId;

            return participant;
        }

        /// <summary>
        /// Creates the seat of a guest.
        /// </summary>
        /// <param name="gameSessionId">Identifier of the match; must be positive.</param>
        /// <param name="guestSessionId">Identifier of the guest session; must be positive.</param>
        /// <returns>The new seat, with the initial score.</returns>
        public static GameParticipant ForGuest(int gameSessionId, int guestSessionId)
        {
            ValidateIdentifier(gameSessionId, nameof(gameSessionId));
            ValidateIdentifier(guestSessionId, nameof(guestSessionId));

            GameParticipant participant = new GameParticipant(gameSessionId);
            participant.GuestSessionId = guestSessionId;

            return participant;
        }

        /// <summary>
        /// Replaces the cards in hand.
        /// </summary>
        /// <param name="cardsInHand">Cards in hand, as JSON text; required.</param>
        public void ChangeHand(string cardsInHand)
        {
            if (string.IsNullOrWhiteSpace(cardsInHand))
            {
                throw new ArgumentException("The hand is required.", nameof(cardsInHand));
            }

            CardsInHand = cardsInHand;
        }

        /// <summary>
        /// Replaces the points of the participant.
        /// </summary>
        /// <param name="score">New points; not negative.</param>
        public void RecordScore(int score)
        {
            if (score < InitialScore)
            {
                throw new ArgumentOutOfRangeException(nameof(score), "The score cannot be negative.");
            }

            Score = score;
        }

        // An identifier that is not positive cannot point to a stored row.
        private static void ValidateIdentifier(int identifier, string parameterName)
        {
            if (identifier <= 0)
            {
                throw new ArgumentException("The identifier must be positive.", parameterName);
            }
        }
    }
}
