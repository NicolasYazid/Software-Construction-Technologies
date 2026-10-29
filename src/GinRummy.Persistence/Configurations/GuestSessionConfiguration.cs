using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the GuestSession entity to the GuestSession table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class GuestSessionConfiguration : EntityTypeConfiguration<GuestSession>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public GuestSessionConfiguration()
        {
            ToTable("GuestSession");
            HasKey(guestSession => guestSession.GuestSessionId);
            Property(guestSession => guestSession.GuestSessionId).HasColumnName("guest_session_id");
            Property(guestSession => guestSession.TemporalDisplayName)
                .HasColumnName("temporal_display_name")
                .IsRequired()
                .HasMaxLength(20);
            Property(guestSession => guestSession.CreatedAt).HasColumnName("created_at");
        }
    }
}
