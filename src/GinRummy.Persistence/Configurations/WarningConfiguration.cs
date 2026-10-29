using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the Warning entity to the Warning table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class WarningConfiguration : EntityTypeConfiguration<Warning>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public WarningConfiguration()
        {
            ToTable("Warning");
            HasKey(warning => warning.WarningId);
            Property(warning => warning.WarningId).HasColumnName("warning_id");
            Property(warning => warning.PlayerId).HasColumnName("player_id");
            Property(warning => warning.ReportId).HasColumnName("report_id");
            Property(warning => warning.BanId).HasColumnName("ban_id");
            Property(warning => warning.IssuedAt).HasColumnName("issued_at");
        }
    }
}
