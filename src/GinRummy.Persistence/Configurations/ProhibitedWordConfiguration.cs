using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the ProhibitedWord entity to the ProhibitedWord table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class ProhibitedWordConfiguration : EntityTypeConfiguration<ProhibitedWord>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public ProhibitedWordConfiguration()
        {
            ToTable("ProhibitedWord");
            HasKey(prohibitedWord => prohibitedWord.ProhibitedWordId);
            Property(prohibitedWord => prohibitedWord.ProhibitedWordId).HasColumnName("prohibited_word_id");
            Property(prohibitedWord => prohibitedWord.Term)
                .HasColumnName("term")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(100);
            HasMany(prohibitedWord => prohibitedWord.Locales)
                .WithRequired()
                .HasForeignKey(locale => locale.ProhibitedWordId);
        }
    }
}
