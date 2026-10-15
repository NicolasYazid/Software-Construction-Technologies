using System;
using System.Collections.Generic;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A match between players, mapped to the GameSession table. The cards of the deck and of
    /// the discard pile are kept as the JSON text the database stores; the Domain does not
    /// read them, because the rules of the game decide what they contain. Protects its own
    /// invariants: it moves only through valid stages and it never ends before it started.
    /// </summary>
    public class GameSession
    {
        /// <summary>
        /// Creates a new match that waits for its players.
        /// </summary>
        public GameSession()
        {
            GameStateName = GameState.Waiting.ToString();
            Participants = new List<GameParticipant>();
        }

        /// <summary>
        /// Gets the internal, autonumeric identifier.
        /// </summary>
        public int GameSessionId { get; private set; }

        /// <summary>
        /// Gets the stage exactly as the game_state column stores it.
        /// </summary>
        public string GameStateName { get; private set; }

        /// <summary>
        /// Gets the cards left in the deck, as JSON text. Null until the cards are dealt.
        /// </summary>
        public string DeckState { get; private set; }

        /// <summary>
        /// Gets the cards in the discard pile, as JSON text. Null until the cards are dealt.
        /// </summary>
        public string DiscardPileState { get; private set; }

        /// <summary>
        /// Gets the identifier of the participant whose turn it is. Null while nobody plays.
        /// </summary>
        public int? CurrentTurn { get; private set; }

        /// <summary>
        /// Gets the moment the match started, in UTC. Null while it waits.
        /// </summary>
        public DateTime? StartedAt { get; private set; }

        /// <summary>
        /// Gets the moment the match ended, in UTC. Null while it is not over.
        /// </summary>
        public DateTime? EndedAt { get; private set; }

        /// <summary>
        /// Gets the players and guests that take part in the match.
        /// </summary>
        public virtual ICollection<GameParticipant> Participants { get; private set; }

        /// <summary>
        /// Gets the stage of the match.
        /// </summary>
        public GameState State
        {
            get { return (GameState)Enum.Parse(typeof(GameState), GameStateName); }
        }

        /// <summary>
        /// Starts a match that was waiting.
        /// </summary>
        /// <param name="startedAt">Moment the match starts, in UTC.</param>
        /// <exception cref="InvalidOperationException">Thrown when the match is not waiting.</exception>
        public void Start(DateTime startedAt)
        {
            RequireState(GameState.Waiting);

            GameStateName = GameState.InProgress.ToString();
            StartedAt = startedAt;
        }

        /// <summary>
        /// Stops a match in progress while a disconnected player tries to come back.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the match is not in progress.</exception>
        public void Pause()
        {
            RequireState(GameState.InProgress);

            GameStateName = GameState.Paused.ToString();
        }

        /// <summary>
        /// Continues a paused match.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the match is not paused.</exception>
        public void Resume()
        {
            RequireState(GameState.Paused);

            GameStateName = GameState.InProgress.ToString();
        }

        /// <summary>
        /// Ends a match that was in progress or paused.
        /// </summary>
        /// <param name="endedAt">Moment the match ended, in UTC; not before it started.</param>
        /// <exception cref="InvalidOperationException">Thrown when the match is not in progress or paused.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the end is before the start.</exception>
        public void Finish(DateTime endedAt)
        {
            if ((State != GameState.InProgress) && (State != GameState.Paused))
            {
                throw new InvalidOperationException("Only a match in progress or paused can finish.");
            }

            if (endedAt < StartedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(endedAt), "A match cannot end before it started.");
            }

            GameStateName = GameState.Finished.ToString();
            EndedAt = endedAt;
        }

        /// <summary>
        /// Replaces the saved cards of the deck and of the discard pile.
        /// </summary>
        /// <param name="deckState">Cards left in the deck, as JSON text; required.</param>
        /// <param name="discardPileState">Cards in the discard pile, as JSON text; required.</param>
        /// <exception cref="InvalidOperationException">Thrown when the match is already over.</exception>
        public void SaveCardState(string deckState, string discardPileState)
        {
            RejectWhenFinished();
            ValidateRequiredText(deckState, nameof(deckState));
            ValidateRequiredText(discardPileState, nameof(discardPileState));

            DeckState = deckState;
            DiscardPileState = discardPileState;
        }

        /// <summary>
        /// Gives the turn to a participant.
        /// </summary>
        /// <param name="gameParticipantId">Identifier of the participant who plays next; must be positive.</param>
        /// <exception cref="InvalidOperationException">Thrown when the match is already over.</exception>
        public void GiveTurnTo(int gameParticipantId)
        {
            RejectWhenFinished();
            if (gameParticipantId <= 0)
            {
                throw new ArgumentException("Participant id must be positive.", nameof(gameParticipantId));
            }

            CurrentTurn = gameParticipantId;
        }

        // A stage change is valid only from one specific stage, so a match cannot skip stages.
        private void RequireState(GameState expected)
        {
            if (State != expected)
            {
                throw new InvalidOperationException("The match is not in the " + expected + " stage.");
            }
        }

        // A finished match is a record; nothing about it changes anymore.
        private void RejectWhenFinished()
        {
            if (State == GameState.Finished)
            {
                throw new InvalidOperationException("The match is already over.");
            }
        }

        // The cards are stored as text, so an empty value would hide a lost deck.
        private static void ValidateRequiredText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("The value is required.", parameterName);
            }
        }
    }
}
