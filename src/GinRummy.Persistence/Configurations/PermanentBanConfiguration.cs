using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities.PlayerModeration;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the PermanentBan entity to the PermanentBan table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class PermanentBanConfiguration : EntityTypeConfiguration<PermanentBan>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public PermanentBanConfiguration()
        {
            ToTable("PermanentBan");
            HasKey(permanentBan => permanentBan.PermanentBanId);
            Property(permanentBan => permanentBan.PermanentBanId).HasColumnName("permanent_ban_id");
            Property(permanentBan => permanentBan.PlayerAccountId).HasColumnName("player_account_id");
            Property(permanentBan => permanentBan.SanctionId).HasColumnName("sanction_id");
            Property(permanentBan => permanentBan.ReasonKey)
                .HasColumnName("reason_key")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(PermanentBan.KeyMaxLength);
            Property(permanentBan => permanentBan.MessageKey)
                .HasColumnName("message_key")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(PermanentBan.KeyMaxLength);
            Property(permanentBan => permanentBan.CreatedAt).HasColumnName("created_at");
        }
    }
}
