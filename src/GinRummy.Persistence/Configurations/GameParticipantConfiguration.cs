using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the GameParticipant entity to the GameParticipant table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure. The hand is a JSON column that the driver returns
    /// as text, so it maps as a plain string. The score lives in the column that keeps the name puntuation.
    /// </summary>
    internal class GameParticipantConfiguration : EntityTypeConfiguration<GameParticipant>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public GameParticipantConfiguration()
        {
            ToTable("GameParticipant");
            HasKey(participant => participant.GameParticipantId);
            Property(participant => participant.GameParticipantId).HasColumnName("game_participant_id");
            Property(participant => participant.GameSessionId).HasColumnName("game_session_id");
            Property(participant => participant.PlayerId).HasColumnName("player_id");
            Property(participant => participant.GuestSessionId).HasColumnName("guest_session_id");
            Property(participant => participant.CardsInHand).HasColumnName("cards_in_hand");
            Property(participant => participant.Score).HasColumnName("puntuation");
            Ignore(participant => participant.IsGuest);
            HasOptional(participant => participant.Player).WithMany().HasForeignKey(participant => participant.PlayerId)
                .WillCascadeOnDelete(false);
            HasOptional(participant => participant.GuestSession).WithMany().HasForeignKey(participant => participant.GuestSessionId)
                .WillCascadeOnDelete(false);
        }
    }
}
