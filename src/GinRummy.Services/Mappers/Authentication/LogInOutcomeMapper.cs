using GinRummy.Contracts.DataContracts.Authentication;
using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Services.Mappers.Authentication
{
    public static class LogInOutcomeMapper
    {
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
