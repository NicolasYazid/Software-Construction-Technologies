using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the ProhibitedWordLocale entity to the ProhibitedWordLocale table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class ProhibitedWordLocaleConfiguration : EntityTypeConfiguration<ProhibitedWordLocale>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public ProhibitedWordLocaleConfiguration()
        {
            ToTable("ProhibitedWordLocale");
            HasKey(prohibitedWordLocale => new { prohibitedWordLocale.ProhibitedWordId, prohibitedWordLocale.LocaleId });
            Property(prohibitedWordLocale => prohibitedWordLocale.ProhibitedWordId)
                .HasColumnName("prohibited_word_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(prohibitedWordLocale => prohibitedWordLocale.LocaleId)
                .HasColumnName("locale_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }
    }
}
