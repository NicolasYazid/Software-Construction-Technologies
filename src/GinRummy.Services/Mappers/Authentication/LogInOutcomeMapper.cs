using System;

using GinRummy.Contracts.DataContracts.Authentication;
using GinRummy.Domain.Entities.Authentication;
using ContractLogInStatus = GinRummy.Contracts.DataContracts.Authentication.LogInStatus;
using DomainLogInStatus = GinRummy.Domain.Entities.Authentication.LogInStatus;

namespace GinRummy.Services.Mappers.Authentication
{
    public static class LogInOutcomeMapper
    {
        public static LogInOutcome MapToLogInOutcome(LogInResult result)
        {
            LogInOutcome outcome = new LogInOutcome
            {
                Status = (ContractLogInStatus)Enum.Parse(typeof(ContractLogInStatus), result.Status.ToString()),
                IsTwoFactorRequired = result.IsTwoFactorRequired,
                AccessToken = string.Empty,
                Username = string.Empty,
                LocaleCode = string.Empty,
                SanctionReasonName = result.SanctionReasonName ?? string.Empty,
                SanctionExpiresAt = result.SanctionExpiresAt
            };
            bool isSignedIn = (result.Status == DomainLogInStatus.Accepted) && !result.IsTwoFactorRequired ? true : false;
            if (isSignedIn)
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
