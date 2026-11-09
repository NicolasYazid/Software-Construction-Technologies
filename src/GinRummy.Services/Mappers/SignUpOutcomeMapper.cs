using GinRummy.Contracts.DataContracts.Authentication;
using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Services.Mappers
{
    /// <summary>
    /// Turns a created account into the answer the client receives.
    /// </summary>
    public static class SignUpOutcomeMapper
    {
        /// <summary>
        /// Maps a created account. Only the email and the status of its code travel, because the client
        /// still has to verify the email before it can sign in.
        /// </summary>
        /// <param name="result">Outcome of the sign-up.</param>
        /// <returns>The answer for the client.</returns>
        public static SignUpOutcome MapToSignUpOutcome(SignUpResult result)
        {
            SignUpOutcome outcome = new SignUpOutcome
            {
                Email = result.Account.Email,
                IsVerificationCodeSent = result.IsVerificationCodeSent
            };

            return outcome;
        }
    }
}
