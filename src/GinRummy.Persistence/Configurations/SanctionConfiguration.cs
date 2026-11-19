using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities.PlayerModeration;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the Sanction entity to the Sanction table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class SanctionConfiguration : EntityTypeConfiguration<Sanction>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public SanctionConfiguration()
        {
            ToTable("Sanction");
            HasKey(sanction => sanction.SanctionId);
            Property(sanction => sanction.SanctionId).HasColumnName("sanction_id");
            Property(sanction => sanction.PlayerAccountId).HasColumnName("player_account_id");
            Property(sanction => sanction.WarningId).HasColumnName("warning_id");
            Property(sanction => sanction.ReasonKey)
                .HasColumnName("reason_key")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(Sanction.KeyMaxLength);
            Property(sanction => sanction.MessageKey)
                .HasColumnName("message_key")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(Sanction.KeyMaxLength);
            Property(sanction => sanction.CreatedAt).HasColumnName("created_at");
            Property(sanction => sanction.StartsAt).HasColumnName("starts_at");
            Property(sanction => sanction.EndsAt).HasColumnName("ends_at");
            Ignore(sanction => sanction.BlocksAllChats);
        }
    }
}
