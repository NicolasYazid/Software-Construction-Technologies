using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities.PlayerModeration;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the ReasonBanLocale entity to the ReasonBanLocale table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class ReasonBanLocaleConfiguration : EntityTypeConfiguration<ReasonBanLocale>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public ReasonBanLocaleConfiguration()
        {
            ToTable("ReasonBanLocale");
            HasKey(reasonBanLocale => new { reasonBanLocale.ReasonBanId, reasonBanLocale.LocaleId });
            Property(reasonBanLocale => reasonBanLocale.ReasonBanId)
                .HasColumnName("reason_ban_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(reasonBanLocale => reasonBanLocale.LocaleId)
                .HasColumnName("locale_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(reasonBanLocale => reasonBanLocale.Name)
                .HasColumnName("name")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(ReasonBanLocale.NameMaxLength);
        }
    }
}
