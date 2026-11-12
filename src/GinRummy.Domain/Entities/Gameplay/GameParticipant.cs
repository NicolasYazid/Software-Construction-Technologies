using System;

using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Domain.Entities.Gameplay
{
    public class GameParticipant
    {
        private const int InitialScore = 0;

        protected GameParticipant()
        {
        }

        private GameParticipant(int gameSessionId)
        {
            GameSessionId = gameSessionId;
            Score = InitialScore;
        }

        public int GameParticipantId { get; private set; }

        public int GameSessionId { get; private set; }

        public int? PlayerAccountId { get; private set; }

        public int? GuestSessionId { get; private set; }

        public string CardsInHand { get; private set; }

        public int Score { get; private set; }

        public virtual PlayerAccount PlayerAccount { get; private set; }

        public virtual GuestSession GuestSession { get; private set; }

        public bool IsGuest
        {
            get { return GuestSessionId.HasValue; }
        }

        public static GameParticipant ForPlayer(int gameSessionId, int playerAccountId)
        {
            ValidateIdentifier(gameSessionId, nameof(gameSessionId));
            ValidateIdentifier(playerAccountId, nameof(playerAccountId));

            GameParticipant participant = new GameParticipant(gameSessionId);
            participant.PlayerAccountId = playerAccountId;

            return participant;
        }

        public static GameParticipant ForGuest(int gameSessionId, int guestSessionId)
        {
            ValidateIdentifier(gameSessionId, nameof(gameSessionId));
            ValidateIdentifier(guestSessionId, nameof(guestSessionId));

            GameParticipant participant = new GameParticipant(gameSessionId);
            participant.GuestSessionId = guestSessionId;

            return participant;
        }

        public void ChangeHand(string cardsInHand)
        {
            if (string.IsNullOrWhiteSpace(cardsInHand))
            {
                throw new ArgumentException("The hand is required.", nameof(cardsInHand));
            }

            CardsInHand = cardsInHand;
        }

        public void RecordScore(int score)
        {
            if (score < InitialScore)
            {
                throw new ArgumentOutOfRangeException(nameof(score), "The score cannot be negative.");
            }

            Score = score;
        }

        private static void ValidateIdentifier(int identifier, string parameterName)
        {
            if (identifier <= 0)
            {
                throw new ArgumentException("The identifier must be positive.", parameterName);
            }
        }
    }
}
