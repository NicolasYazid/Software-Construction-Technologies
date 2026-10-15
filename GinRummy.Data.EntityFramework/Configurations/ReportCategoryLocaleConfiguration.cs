using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Configurations
{
    /// <summary>
    /// Maps the ReportCategoryLocale entity to the ReportCategoryLocale table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class ReportCategoryLocaleConfiguration : EntityTypeConfiguration<ReportCategoryLocale>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public ReportCategoryLocaleConfiguration()
        {
            ToTable("ReportCategoryLocale");
            HasKey(reportCategoryLocale => new { reportCategoryLocale.ReportCategoryId, reportCategoryLocale.LocaleId });
            Property(reportCategoryLocale => reportCategoryLocale.ReportCategoryId)
                .HasColumnName("report_category_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(reportCategoryLocale => reportCategoryLocale.LocaleId)
                .HasColumnName("locale_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(reportCategoryLocale => reportCategoryLocale.Name)
                .HasColumnName("name")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(100);
        }
    }
}
