namespace GinRummy.Domain.Entities
{
    // The numbers must match the rows already seeded in the Purpose table.
    public enum VerificationPurpose
    {
        CreateAccount = 1,
        PasswordRecovery = 2
    }
}
