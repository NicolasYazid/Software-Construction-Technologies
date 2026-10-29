using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Configurations
{
    /// <summary>
    /// Maps the RankLocale entity to the RankLocale table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class RankLocaleConfiguration : EntityTypeConfiguration<RankLocale>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public RankLocaleConfiguration()
        {
            ToTable("RankLocale");
            HasKey(rankLocale => new { rankLocale.RankId, rankLocale.LocaleId });
            Property(rankLocale => rankLocale.RankId)
                .HasColumnName("rank_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(rankLocale => rankLocale.LocaleId)
                .HasColumnName("locale_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(rankLocale => rankLocale.Name)
                .HasColumnName("name")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(100);
        }
    }
}
