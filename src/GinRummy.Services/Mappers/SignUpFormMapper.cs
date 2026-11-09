using GinRummy.Contracts.DataContracts.Authentication;
using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Services.Mappers
{
    /// <summary>
    /// Turns the new account the client sent into the form the sign-up checks.
    /// </summary>
    public static class SignUpFormMapper
    {
        /// <summary>
        /// Maps the new account. A missing account becomes an empty form, which the sign-up rejects as a
        /// form with empty fields.
        /// </summary>
        /// <param name="newAccount">What the client sent, or null.</param>
        /// <returns>The form for the sign-up.</returns>
        public static SignUpForm MapToSignUpForm(NewAccount newAccount)
        {
            return new SignUpForm(newAccount?.Email, newAccount?.Username, newAccount?.Password);
        }
    }
}
