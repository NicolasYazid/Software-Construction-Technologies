using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities.PlayerModeration;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the Report entity to the Report table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class ReportConfiguration : EntityTypeConfiguration<Report>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public ReportConfiguration()
        {
            ToTable("Report");
            HasKey(report => report.ReportId);
            Property(report => report.ReportId).HasColumnName("report_id");
            Property(report => report.ReporterPlayerAccountId).HasColumnName("reporter_player_account_id");
            Property(report => report.ReportedPlayerAccountId).HasColumnName("reported_player_account_id");
            Property(report => report.ReportedContent)
                .HasColumnName("reported_content")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(ReportedMessage.ContentMaxLength);
            Property(report => report.MessageSentAt).HasColumnName("message_sent_at");
            Property(report => report.OriginName)
                .HasColumnName("origin")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(Report.OriginNameMaxLength);
            Property(report => report.CreatedAt)
                .HasColumnName("created_at")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            Ignore(report => report.Origin);
        }
    }
}
