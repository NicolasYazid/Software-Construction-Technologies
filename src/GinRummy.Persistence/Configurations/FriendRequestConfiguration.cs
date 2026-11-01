using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities.Friendship;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the FriendRequest entity to the FriendRequest table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class FriendRequestConfiguration : EntityTypeConfiguration<FriendRequest>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public FriendRequestConfiguration()
        {
            ToTable("FriendRequest");
            HasKey(friendRequest => friendRequest.FriendRequestId);
            Property(friendRequest => friendRequest.FriendRequestId).HasColumnName("friend_request_id");
            Property(friendRequest => friendRequest.RequesterPlayerId).HasColumnName("requester_player_id");
            Property(friendRequest => friendRequest.RecipientPlayerId).HasColumnName("recipient_player_id");
            Property(friendRequest => friendRequest.CreatedAt).HasColumnName("created_at");
            HasRequired(friendRequest => friendRequest.Requester).WithMany().HasForeignKey(friendRequest => friendRequest.RequesterPlayerId)
                .WillCascadeOnDelete(false);
            HasRequired(friendRequest => friendRequest.Recipient).WithMany().HasForeignKey(friendRequest => friendRequest.RecipientPlayerId)
                .WillCascadeOnDelete(false);
        }
    }
}
