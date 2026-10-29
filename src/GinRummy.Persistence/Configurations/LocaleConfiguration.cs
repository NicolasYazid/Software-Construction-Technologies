using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the Locale entity to the Locale table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class LocaleConfiguration : EntityTypeConfiguration<Locale>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public LocaleConfiguration()
        {
            ToTable("Locale");
            HasKey(locale => locale.LocaleId);
            Property(locale => locale.LocaleId).HasColumnName("locale_id");
            Property(locale => locale.LocaleCode)
                .HasColumnName("locale_code")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(Locale.LocaleCodeMaxLength);
            Property(locale => locale.DisplayName)
                .HasColumnName("display_name")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(Locale.DisplayNameMaxLength);
        }
    }
}
