using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Configurations
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
            Property(report => report.ReporterPlayerId).HasColumnName("reporter_player_id");
            Property(report => report.ReportedPlayerId).HasColumnName("reported_player_id");
            Property(report => report.ReportCategoryId).HasColumnName("report_category_id");
            Property(report => report.ReportStatusId).HasColumnName("report_status_id");
            Property(report => report.CreatedAt).HasColumnName("created_at");
            HasRequired(report => report.ReportCategory).WithMany().HasForeignKey(report => report.ReportCategoryId);
            HasRequired(report => report.ReportStatus).WithMany().HasForeignKey(report => report.ReportStatusId);
        }
    }
}
