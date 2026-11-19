using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities.PlayerCareer;

namespace GinRummy.Persistence.Configurations
{
    internal class PlayerStatsConfiguration : EntityTypeConfiguration<PlayerStats>
    {
        public PlayerStatsConfiguration()
        {
            ToTable("PlayerStats");
            HasKey(playerStats => playerStats.PlayerAccountId);
            Property(playerStats => playerStats.PlayerAccountId)
                .HasColumnName("player_account_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(playerStats => playerStats.Wins).HasColumnName("wins");
            Property(playerStats => playerStats.Losses).HasColumnName("losses");
            Property(playerStats => playerStats.Score).HasColumnName("score");
            Property(playerStats => playerStats.MatchesPlayed)
                .HasColumnName("matches_played")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            Ignore(playerStats => playerStats.WinRate);
        }
    }
}
