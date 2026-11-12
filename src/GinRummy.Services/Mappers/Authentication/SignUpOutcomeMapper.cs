using GinRummy.Contracts.DataContracts.Authentication;
using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Services.Mappers.Authentication
{
    public static class SignUpOutcomeMapper
    {
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
