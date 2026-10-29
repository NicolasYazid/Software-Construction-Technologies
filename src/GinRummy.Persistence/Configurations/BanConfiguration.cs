using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the Ban entity to the Ban table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class BanConfiguration : EntityTypeConfiguration<Ban>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public BanConfiguration()
        {
            ToTable("Ban");
            HasKey(ban => ban.BanId);
            Property(ban => ban.BanId).HasColumnName("ban_id");
            Property(ban => ban.PlayerId).HasColumnName("player_id");
            Property(ban => ban.ReasonBanId).HasColumnName("reason_ban_id");
            Property(ban => ban.IssuedAt).HasColumnName("issued_at");
            Property(ban => ban.ExpiresAt).HasColumnName("expires_at");
            Ignore(ban => ban.IsPermanent);
            HasRequired(ban => ban.ReasonBan).WithMany().HasForeignKey(ban => ban.ReasonBanId);
        }
    }
}
