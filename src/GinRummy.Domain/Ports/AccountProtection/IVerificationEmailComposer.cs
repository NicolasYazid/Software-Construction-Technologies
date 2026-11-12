using GinRummy.Domain.Entities.AccountProtection;

namespace GinRummy.Domain.Ports.AccountProtection
{
    public interface IVerificationEmailComposer
    {
        string ComposeSubject(VerificationPurpose purpose, string localeCode);

        string ComposeBody(VerificationPurpose purpose, string plainCode, string localeCode);
    }
}
