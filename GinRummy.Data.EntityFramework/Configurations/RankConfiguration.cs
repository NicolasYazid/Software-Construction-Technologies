using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Configurations
{
    /// <summary>
    /// Maps the Rank entity to the Rank table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class RankConfiguration : EntityTypeConfiguration<Rank>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public RankConfiguration()
        {
            ToTable("Rank");
            HasKey(rank => rank.RankId);
            Property(rank => rank.RankId).HasColumnName("rank_id");
            Property(rank => rank.Name).HasColumnName("name").IsRequired().IsUnicode(false).HasMaxLength(100);
            Property(rank => rank.MinimumScore).HasColumnName("minimum_score");
            Property(rank => rank.MaximumScore).HasColumnName("maximum_score");
            HasMany(rank => rank.Translations)
                .WithRequired()
                .HasForeignKey(translation => translation.RankId);
        }
    }
}
