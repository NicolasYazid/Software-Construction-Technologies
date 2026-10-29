using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Configurations
{
    /// <summary>
    /// Maps the PlayerAccount entity to the PlayerAccount table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class PlayerAccountConfiguration : EntityTypeConfiguration<PlayerAccount>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public PlayerAccountConfiguration()
        {
            ToTable("PlayerAccount");
            HasKey(playerAccount => playerAccount.PlayerId);
            Property(playerAccount => playerAccount.PlayerId)
                .HasColumnName("player_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(playerAccount => playerAccount.Email)
                .HasColumnName("email")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(254);
            Property(playerAccount => playerAccount.PasswordHash)
                .HasColumnName("password")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(255);
            Property(playerAccount => playerAccount.LastLoginAt).HasColumnName("last_login_at");
            Property(playerAccount => playerAccount.IsEmailVerified).HasColumnName("is_email_verified");
            Property(playerAccount => playerAccount.IsTwoFactorEnabled).HasColumnName("is_two_factor_enabled");
            HasRequired(playerAccount => playerAccount.Player).WithOptional();
        }
    }
}
