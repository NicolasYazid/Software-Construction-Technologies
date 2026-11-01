using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities.Friendship;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the FriendShip entity to the FriendShip table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class FriendShipConfiguration : EntityTypeConfiguration<FriendShip>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public FriendShipConfiguration()
        {
            ToTable("FriendShip");
            HasKey(friendShip => friendShip.FriendShipId);
            Property(friendShip => friendShip.FriendShipId).HasColumnName("friend_ship_id");
            Property(friendShip => friendShip.PlayerAId).HasColumnName("player_a_id");
            Property(friendShip => friendShip.PlayerBId).HasColumnName("player_b_id");
            HasRequired(friendShip => friendShip.PlayerA).WithMany().HasForeignKey(friendShip => friendShip.PlayerAId)
                .WillCascadeOnDelete(false);
            HasRequired(friendShip => friendShip.PlayerB).WithMany().HasForeignKey(friendShip => friendShip.PlayerBId)
                .WillCascadeOnDelete(false);
        }
    }
}
