using System;
using System.Collections.Generic;

namespace GinRummy.Domain.Entities.Gameplay
{
    public class GameSession
    {
        public const int GameStateNameMaxLength = 20;

        public GameSession()
        {
            GameStateName = GameState.Waiting.ToString();
            Participants = new List<GameParticipant>();
        }

        public int GameSessionId { get; private set; }

        public string GameStateName { get; private set; }

        public string DeckState { get; private set; }

        public string DiscardPileState { get; private set; }

        public int? CurrentTurn { get; private set; }

        public DateTime? StartedAt { get; private set; }

        public DateTime? EndedAt { get; private set; }

        public virtual ICollection<GameParticipant> Participants { get; private set; }

        public GameState State
        {
            get { return (GameState)Enum.Parse(typeof(GameState), GameStateName); }
        }

        public void Start(DateTime startedAt)
        {
            RequireState(GameState.Waiting);

            GameStateName = GameState.InProgress.ToString();
            StartedAt = startedAt;
        }

        public void Pause()
        {
            RequireState(GameState.InProgress);

            GameStateName = GameState.Paused.ToString();
        }

        public void Resume()
        {
            RequireState(GameState.Paused);

            GameStateName = GameState.InProgress.ToString();
        }

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

        public void SaveCardState(string deckState, string discardPileState)
        {
            RejectWhenFinished();
            ValidateRequiredText(deckState, nameof(deckState));
            ValidateRequiredText(discardPileState, nameof(discardPileState));

            DeckState = deckState;
            DiscardPileState = discardPileState;
        }

        public void GiveTurnTo(int gameParticipantId)
        {
            RejectWhenFinished();
            if (gameParticipantId <= 0)
            {
                throw new ArgumentException("Participant id must be positive.", nameof(gameParticipantId));
            }

            CurrentTurn = gameParticipantId;
        }

        private void RequireState(GameState expected)
        {
            if (State != expected)
            {
                throw new InvalidOperationException("The match is not in the " + expected + " stage.");
            }
        }

        private void RejectWhenFinished()
        {
            if (State == GameState.Finished)
            {
                throw new InvalidOperationException("The match is already over.");
            }
        }

        private static void ValidateRequiredText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("The value is required.", parameterName);
            }
        }
    }
}
