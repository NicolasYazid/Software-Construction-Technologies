using System;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Dtos
{
    public class NewVerificationCodeDto
    {
        public int PlayerId { get; set; }
        public VerificationPurpose Purpose { get; set; }
        public string CodeHash { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
