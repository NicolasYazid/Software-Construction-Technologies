using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Configurations
{
    /// <summary>
    /// Maps the ReportCategory entity to the ReportCategory table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class ReportCategoryConfiguration : EntityTypeConfiguration<ReportCategory>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public ReportCategoryConfiguration()
        {
            ToTable("ReportCategory");
            HasKey(reportCategory => reportCategory.ReportCategoryId);
            Property(reportCategory => reportCategory.ReportCategoryId).HasColumnName("report_category_id");
            Property(reportCategory => reportCategory.Name)
                .HasColumnName("name")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(100);
            HasMany(reportCategory => reportCategory.Translations)
                .WithRequired()
                .HasForeignKey(translation => translation.ReportCategoryId);
        }
    }
}
