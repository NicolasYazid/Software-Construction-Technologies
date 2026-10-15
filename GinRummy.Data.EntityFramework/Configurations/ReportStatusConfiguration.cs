using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Configurations
{
    /// <summary>
    /// Maps the ReportStatus entity to the ReportStatus table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class ReportStatusConfiguration : EntityTypeConfiguration<ReportStatus>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public ReportStatusConfiguration()
        {
            ToTable("ReportStatus");
            HasKey(reportStatus => reportStatus.ReportStatusId);
            Property(reportStatus => reportStatus.ReportStatusId).HasColumnName("report_status_id");
            Property(reportStatus => reportStatus.Name)
                .HasColumnName("name")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(100);
        }
    }
}
