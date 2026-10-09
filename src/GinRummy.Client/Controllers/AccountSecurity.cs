using GinRummy.Domain.Security;

namespace GinRummy.Client.Controllers
{
    // The three security services of the sign-up travel together so that its controller stays within three constructor parameters.
    public class AccountSecurity
    {
        public AccountSecurity(
            IPasswordHasher passwordHasher,
            IVerificationCodeGenerator codeGenerator,
            IVerificationCodeHasher codeHasher)
        {
            PasswordHasher = passwordHasher;
            CodeGenerator = codeGenerator;
            CodeHasher = codeHasher;
        }

        public IPasswordHasher PasswordHasher { get; }
        public IVerificationCodeGenerator CodeGenerator { get; }
        public IVerificationCodeHasher CodeHasher { get; }
    }
}
