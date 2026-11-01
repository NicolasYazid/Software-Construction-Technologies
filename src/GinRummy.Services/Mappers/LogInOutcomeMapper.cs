using GinRummy.Contracts.DataContracts;
using GinRummy.Domain.Entities;

namespace GinRummy.Services.Mappers
{
    /// <summary>
    /// Turns the outcome of a sign-in into the answer the client receives.
    /// </summary>
    public static class LogInOutcomeMapper
    {
        /// <summary>
        /// Maps a sign-in outcome. While the second factor is pending, only that notice travels, because the
        /// identity of the account must not reach a client that has not confirmed the code yet.
        /// </summary>
        /// <param name="result">Outcome of the sign-in.</param>
        /// <returns>The answer for the client.</returns>
        public static LogInOutcome MapToLogInOutcome(LogInResult result)
        {
            LogInOutcome outcome = new LogInOutcome
            {
                IsTwoFactorRequired = true,
                Username = string.Empty
            };
            if (!result.IsTwoFactorRequired)
            {
                outcome = MapToSignedInOutcome(result);
            }

            return outcome;
        }

        private static LogInOutcome MapToSignedInOutcome(LogInResult result)
        {
            Player player = result.Account.Player;
            LogInOutcome outcome = new LogInOutcome
            {
                IsTwoFactorRequired = false,
                PlayerSessionId = result.OpenedSession.PlayerSessionId,
                PlayerId = player.PlayerId,
                Username = player.Username,
                PublicTag = player.PublicTag
            };

            return outcome;
        }
    }
}
