using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the SocialMedia entity to the SocialMedia table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class SocialMediaConfiguration : EntityTypeConfiguration<SocialMedia>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public SocialMediaConfiguration()
        {
            ToTable("SocialMedia");
            HasKey(socialMedia => socialMedia.SocialMediaId);
            Property(socialMedia => socialMedia.SocialMediaId).HasColumnName("social_media_id");
            Property(socialMedia => socialMedia.PlayerId).HasColumnName("player_id");
            Property(socialMedia => socialMedia.PlatformId).HasColumnName("platform_id");
            Property(socialMedia => socialMedia.Url)
                .HasColumnName("url")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(255);
            HasRequired(socialMedia => socialMedia.Platform).WithMany().HasForeignKey(socialMedia => socialMedia.PlatformId);
        }
    }
}
