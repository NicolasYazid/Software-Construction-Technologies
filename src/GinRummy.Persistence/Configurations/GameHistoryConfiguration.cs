using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the GameHistory entity to the GameHistory table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure. The result of the match lives in the column
    /// game_result, where 1 means that the player won.
    /// </summary>
    internal class GameHistoryConfiguration : EntityTypeConfiguration<GameHistory>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public GameHistoryConfiguration()
        {
            ToTable("GameHistory");
            HasKey(history => history.MatchId);
            Property(history => history.MatchId).HasColumnName("match_id");
            Property(history => history.PlayerId).HasColumnName("player_id");
            Property(history => history.OpponentId).HasColumnName("opponent_id");
            Property(history => history.DidWin).HasColumnName("game_result");
            Property(history => history.ScoreEarned).HasColumnName("score_earned");
            Property(history => history.PlayedAt).HasColumnName("played_at");
            HasRequired(history => history.Player).WithMany().HasForeignKey(history => history.PlayerId)
                .WillCascadeOnDelete(false);
            HasRequired(history => history.Opponent).WithMany().HasForeignKey(history => history.OpponentId)
                .WillCascadeOnDelete(false);
        }
    }
}
