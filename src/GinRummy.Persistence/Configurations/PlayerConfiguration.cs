using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the Player entity to the Player table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class PlayerConfiguration : EntityTypeConfiguration<Player>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public PlayerConfiguration()
        {
            ToTable("Player");
            HasKey(player => player.PlayerId);
            Property(player => player.PlayerId).HasColumnName("player_id");
            Property(player => player.PublicTag)
                .HasColumnName("public_tag")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            Property(player => player.Username)
                .HasColumnName("username")
                .IsRequired()
                .HasMaxLength(Player.UsernameMaxLength);
            Property(player => player.LocaleId).HasColumnName("locale_id");
            Property(player => player.CreatedAt).HasColumnName("created_at");
            Property(player => player.IsChatFilterEnabled).HasColumnName("is_chat_filter_enabled");
        }
    }
}
