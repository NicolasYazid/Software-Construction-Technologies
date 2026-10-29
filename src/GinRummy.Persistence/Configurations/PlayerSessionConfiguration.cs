using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the PlayerSession entity to the PlayerSession table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class PlayerSessionConfiguration : EntityTypeConfiguration<PlayerSession>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public PlayerSessionConfiguration()
        {
            ToTable("PlayerSession");
            HasKey(playerSession => playerSession.PlayerSessionId);
            Property(playerSession => playerSession.PlayerSessionId).HasColumnName("player_session_id");
            Property(playerSession => playerSession.PlayerId).HasColumnName("player_id");
            Property(playerSession => playerSession.IsPersistent).HasColumnName("is_persistent");
            Property(playerSession => playerSession.StartedAt).HasColumnName("started_at");
            Property(playerSession => playerSession.EndedAt).HasColumnName("ended_at");
            Ignore(playerSession => playerSession.IsActive);
        }
    }
}
