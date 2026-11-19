namespace GinRummy.Domain.Entities.AccountProtection
{
    public class VerificationRequest
    {
        public VerificationRequest(int playerAccountId, VerificationPurpose purpose)
        {
            PlayerAccountId = playerAccountId;
            Purpose = purpose;
        }

        public int PlayerAccountId { get; private set; }

        public VerificationPurpose Purpose { get; private set; }
    }
}
