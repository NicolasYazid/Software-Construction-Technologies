using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities.Players;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the PlayerProfile entity to the PlayerProfile table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class PlayerProfileConfiguration : EntityTypeConfiguration<PlayerProfile>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public PlayerProfileConfiguration()
        {
            ToTable("PlayerProfile");
            HasKey(playerProfile => playerProfile.PlayerId);
            Property(playerProfile => playerProfile.PlayerId)
                .HasColumnName("player_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(playerProfile => playerProfile.Bio)
                .HasColumnName("bio")
                .IsUnicode(false)
                .HasMaxLength(PlayerProfile.BioMaxLength);
            Property(playerProfile => playerProfile.ProfilePicture)
                .HasColumnName("profile_picture")
                .IsUnicode(false)
                .HasMaxLength(PlayerProfile.ProfilePictureMaxLength);
            HasRequired(playerProfile => playerProfile.PlayerAccount).WithOptional();
            HasMany(playerProfile => playerProfile.SocialMediaLinks)
                .WithRequired()
                .HasForeignKey(link => link.PlayerId);
        }
    }
}
