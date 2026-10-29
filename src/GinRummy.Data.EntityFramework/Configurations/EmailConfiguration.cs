using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Configurations
{
    /// <summary>
    /// Maps the Email entity to the Email table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class EmailConfiguration : EntityTypeConfiguration<Email>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public EmailConfiguration()
        {
            ToTable("Email");
            HasKey(email => email.EmailId);
            Property(email => email.EmailId).HasColumnName("email_id");
            Property(email => email.VerificationCodeId).HasColumnName("verification_code_id");
            Property(email => email.DestinationAddress)
                .HasColumnName("destination_address")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(254);
            Property(email => email.SendAt).HasColumnName("send_at");
            Property(email => email.DeliveryStatusName)
                .HasColumnName("delivery_status")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(20);
            Ignore(email => email.DeliveryStatus);
        }
    }
}
