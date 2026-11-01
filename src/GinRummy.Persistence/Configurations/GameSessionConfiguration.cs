using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities.Game;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the GameSession entity to the GameSession table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure. The deck and the discard pile are JSON columns
    /// that the driver returns as text, so they map as plain strings.
    /// </summary>
    internal class GameSessionConfiguration : EntityTypeConfiguration<GameSession>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public GameSessionConfiguration()
        {
            ToTable("GameSession");
            HasKey(gameSession => gameSession.GameSessionId);
            Property(gameSession => gameSession.GameSessionId).HasColumnName("game_session_id");
            Property(gameSession => gameSession.GameStateName)
                .HasColumnName("game_state")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(GameSession.GameStateNameMaxLength);
            Property(gameSession => gameSession.DeckState).HasColumnName("deck_state");
            Property(gameSession => gameSession.DiscardPileState).HasColumnName("discard_pile_state");
            Property(gameSession => gameSession.CurrentTurn).HasColumnName("current_turn");
            Property(gameSession => gameSession.StartedAt).HasColumnName("started_at");
            Property(gameSession => gameSession.EndedAt).HasColumnName("ended_at");
            Ignore(gameSession => gameSession.State);

            // The turn stays a plain number because the table points to GameParticipant and the participants point back to the match.
            // Mapping both directions would make the inserts circular.
            HasMany(gameSession => gameSession.Participants)
                .WithRequired()
                .HasForeignKey(participant => participant.GameSessionId)
                .WillCascadeOnDelete(false);
        }
    }
}
