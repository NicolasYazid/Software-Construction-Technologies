using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the Challenge entity to the Challenge table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class ChallengeConfiguration : EntityTypeConfiguration<Challenge>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public ChallengeConfiguration()
        {
            ToTable("Challenge");
            HasKey(challenge => challenge.ChallengeId);
            Property(challenge => challenge.ChallengeId).HasColumnName("challenge_id");
            Property(challenge => challenge.ChallengerPlayerId).HasColumnName("challenger_player_id");
            Property(challenge => challenge.ChallengedPlayerId).HasColumnName("challenged_player_id");
            Property(challenge => challenge.StatusName)
                .HasColumnName("status")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(20);
            Property(challenge => challenge.CreatedAt).HasColumnName("created_at");
            Property(challenge => challenge.ExpiresAt).HasColumnName("expires_at");
            Property(challenge => challenge.ResolvedAt).HasColumnName("resolved_at");
            Property(challenge => challenge.GameSessionId).HasColumnName("game_session_id");
            Ignore(challenge => challenge.Status);
            Ignore(challenge => challenge.IsPending);
            HasRequired(challenge => challenge.Challenger).WithMany().HasForeignKey(challenge => challenge.ChallengerPlayerId)
                .WillCascadeOnDelete(false);
            HasRequired(challenge => challenge.Challenged).WithMany().HasForeignKey(challenge => challenge.ChallengedPlayerId)
                .WillCascadeOnDelete(false);
        }
    }
}
