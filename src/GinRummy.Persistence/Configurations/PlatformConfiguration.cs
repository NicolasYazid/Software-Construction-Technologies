using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the Platform entity to the Platform table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class PlatformConfiguration : EntityTypeConfiguration<Platform>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public PlatformConfiguration()
        {
            ToTable("Platform");
            HasKey(platform => platform.PlatformId);
            Property(platform => platform.PlatformId).HasColumnName("platform_id");
            Property(platform => platform.Name)
                .HasColumnName("name")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(Platform.NameMaxLength);
        }
    }
}
