using GinRummy.Contracts.DataContracts.Authentication;
using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Services.Mappers.Authentication
{
    public static class SignUpFormMapper
    {
        public static SignUpForm MapToSignUpForm(NewAccount newAccount)
        {
            return new SignUpForm(newAccount?.Email, newAccount?.Username, newAccount?.Password);
        }
    }
}
