using GinRummy.Contracts.DataContracts.Authentication;
using GinRummy.Domain.Entities.Authentication;

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
                AccessToken = string.Empty,
                Username = string.Empty,
                LocaleCode = string.Empty
            };
            if (!result.IsTwoFactorRequired)
            {
                outcome = MapToSignedInOutcome(result);
            }

            return outcome;
        }

        /// <summary>
        /// Maps the entry of a guest. A guest has no account, so only its name and its token travel.
        /// </summary>
        /// <param name="guestAccess">Session of the guest and its access token.</param>
        /// <returns>The answer for the client.</returns>
        public static LogInOutcome MapToGuestOutcome(GuestAccess guestAccess)
        {
            LogInOutcome outcome = new LogInOutcome
            {
                IsTwoFactorRequired = false,
                AccessToken = guestAccess.AccessToken.Value,
                AccessTokenExpiresAt = guestAccess.AccessToken.ExpiresAt,
                Username = guestAccess.Guest.TemporalDisplayName,
                LocaleCode = string.Empty,
                IsChatFilterEnabled = true
            };

            return outcome;
        }

        private static LogInOutcome MapToSignedInOutcome(LogInResult result)
        {
            PlayerAccount account = result.Account;
            LogInOutcome outcome = new LogInOutcome
            {
                IsTwoFactorRequired = false,
                AccessToken = result.AccessToken.Value,
                AccessTokenExpiresAt = result.AccessToken.ExpiresAt,
                PlayerAccountId = account.PlayerAccountId,
                Username = account.PlayerProfile.Username,
                PublicTag = account.PlayerProfile.PublicTag,
                LocaleCode = account.PlayerPreferences.Locale.LocaleCode,
                IsChatFilterEnabled = account.PlayerPreferences.IsChatFilterEnabled
            };

            return outcome;
        }
    }
}
