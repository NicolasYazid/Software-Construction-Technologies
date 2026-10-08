namespace GinRummy.Domain.Entities
{
    // Reason a verification code was generated.
    // The numbers match the rows already seeded in the Purpose table.
    public enum VerificationPurpose
    {
        CreateAccount = 1,
        PasswordRecovery = 2
    }
}
