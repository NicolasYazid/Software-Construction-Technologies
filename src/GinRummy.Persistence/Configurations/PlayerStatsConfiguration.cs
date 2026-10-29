using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the PlayerStats entity to the PlayerStats table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class PlayerStatsConfiguration : EntityTypeConfiguration<PlayerStats>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public PlayerStatsConfiguration()
        {
            ToTable("PlayerStats");
            HasKey(playerStats => playerStats.PlayerId);
            Property(playerStats => playerStats.PlayerId)
                .HasColumnName("player_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(playerStats => playerStats.Wins).HasColumnName("wins");
            Property(playerStats => playerStats.Losses).HasColumnName("losses");
            Property(playerStats => playerStats.Score).HasColumnName("score");
            Property(playerStats => playerStats.MatchesPlayed)
                .HasColumnName("matches_played")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            Ignore(playerStats => playerStats.WinRate);
            HasRequired(playerStats => playerStats.Player).WithOptional();
        }
    }
}
