using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the Message entity to the Message table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class MessageConfiguration : EntityTypeConfiguration<Message>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public MessageConfiguration()
        {
            ToTable("Message");
            HasKey(message => message.MessageId);
            Property(message => message.MessageId).HasColumnName("message_id");
            Property(message => message.PlayerId).HasColumnName("player_id");
            Property(message => message.Content)
                .HasColumnName("content")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(300);
            Property(message => message.ChannelName)
                .HasColumnName("channel")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(20);
            Property(message => message.GameSessionId).HasColumnName("game_session_id");
            Property(message => message.SentAt).HasColumnName("sent_at");
            Ignore(message => message.Channel);
            HasRequired(message => message.Player).WithMany().HasForeignKey(message => message.PlayerId);
        }
    }
}
