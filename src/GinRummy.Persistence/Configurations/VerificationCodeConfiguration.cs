using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the VerificationCode entity to the VerificationCode table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class VerificationCodeConfiguration : EntityTypeConfiguration<VerificationCode>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public VerificationCodeConfiguration()
        {
            ToTable("VerificationCode");
            HasKey(verificationCode => verificationCode.VerificationCodeId);
            Property(verificationCode => verificationCode.VerificationCodeId)
                .HasColumnName("verification_code_id");
            Property(verificationCode => verificationCode.PlayerId).HasColumnName("player_id");
            Property(verificationCode => verificationCode.Purpose).HasColumnName("purpose_id");
            Property(verificationCode => verificationCode.CodeHash)
                .HasColumnName("code_hash")
                .IsRequired()
                .IsFixedLength()
                .IsUnicode(false)
                .HasMaxLength(64);
            Property(verificationCode => verificationCode.ExpiresAt).HasColumnName("expires_at");
            Property(verificationCode => verificationCode.UsedAt).HasColumnName("used_at");
            Property(verificationCode => verificationCode.Attempts).HasColumnName("attempts");
            Property(verificationCode => verificationCode.CreatedAt).HasColumnName("created_at");
        }
    }
}
