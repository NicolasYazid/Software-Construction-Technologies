using System;
using System.Collections.Generic;

namespace GinRummy.Domain
{
    public class GameMatch
    {
        private readonly Dictionary<int, int> _scoresByPlayerId;
        private bool _isCurrentRoundScored;

        public GameMatch(GameRound firstRound)
        {
            CurrentRound = firstRound ?? throw new ArgumentNullException(nameof(firstRound));
            _scoresByPlayerId = new Dictionary<int, int> { { firstRound.DealerId, 0 }, { firstRound.NonDealerId, 0 } };
        }

        public GameMatch(GameRound firstRound, IDictionary<int, int> scoresByPlayerId)
        {
            CurrentRound = firstRound ?? throw new ArgumentNullException(nameof(firstRound));
            if (scoresByPlayerId == null)
            {
                throw new ArgumentNullException(nameof(scoresByPlayerId));
            }

            _scoresByPlayerId = new Dictionary<int, int>
            {
                { firstRound.DealerId, FindResumedScore(scoresByPlayerId, firstRound.DealerId) },
                { firstRound.NonDealerId, FindResumedScore(scoresByPlayerId, firstRound.NonDealerId) }
            };
        }

        public GameRound CurrentRound { get; private set; }

        public int? WinnerId { get; private set; }

        public bool IsOver
        {
            get { return WinnerId.HasValue; }
        }

        public int GetScore(int playerId)
        {
            int score;
            if (!_scoresByPlayerId.TryGetValue(playerId, out score))
            {
                throw new InvalidMoveException("The player does not take part in this match.");
            }

            return score;
        }

        public void ScoreFinishedRound()
        {
            if ((CurrentRound.Phase != TurnPhase.Finished) || _isCurrentRoundScored)
            {
                throw new InvalidMoveException("Only a finished round that was not scored yet can be scored.");
            }

            _isCurrentRoundScored = true;
            RoundResult result = CurrentRound.Result;
            if (result.WinnerId.HasValue)
            {
                int winnerId = result.WinnerId.Value;
                _scoresByPlayerId[winnerId] += result.Points;
                if (_scoresByPlayerId[winnerId] >= RoundRules.TargetScore)
                {
                    WinnerId = winnerId;
                }
            }
        }

        public void DealNextRound(IList<Card> shuffledDeck)
        {
            if (!_isCurrentRoundScored || IsOver)
            {
                throw new InvalidMoveException("A new round is dealt only after the last one was scored and while the match goes on.");
            }

            CurrentRound = new GameRound(CurrentRound.NonDealerId, CurrentRound.DealerId, shuffledDeck);
            _isCurrentRoundScored = false;
        }

        private static int FindResumedScore(IDictionary<int, int> scoresByPlayerId, int playerId)
        {
            int score;
            bool isFound = scoresByPlayerId.TryGetValue(playerId, out score);
            bool isBelowTarget = (score >= RoundRules.MinimumScore) && (score < RoundRules.TargetScore) ? true : false;
            if (!isFound || !isBelowTarget)
            {
                throw new ArgumentException("A resumed match needs a score below the target for each player.", nameof(scoresByPlayerId));
            }

            return score;
        }
    }
}
