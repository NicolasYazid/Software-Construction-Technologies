namespace GinRummy.Domain.Entities.AccountProtection
{
    public enum VerificationPurpose
    {
        CreateAccount = 1,
        PasswordRecovery = 2,
        TwoFactorSetup = 3,
        TwoFactorLogin = 4,
        PasswordChange = 5,
        EmailChange = 6
    }
}
