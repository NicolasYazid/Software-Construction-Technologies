using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Persistence.Configurations
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
            HasKey(playerAccount => playerAccount.PlayerAccountId);
            Property(playerAccount => playerAccount.PlayerAccountId)
                .HasColumnName("player_account_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(playerAccount => playerAccount.Email)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(PlayerAccount.EmailMaxLength);
            Property(playerAccount => playerAccount.PasswordHash)
                .HasColumnName("password")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(PlayerAccount.PasswordHashMaxLength);
            Property(playerAccount => playerAccount.CreatedAt).HasColumnName("created_at");
            Property(playerAccount => playerAccount.LastLoginAt).HasColumnName("last_login_at");
            Property(playerAccount => playerAccount.IsEmailVerified).HasColumnName("is_email_verified");
            Property(playerAccount => playerAccount.IsTwoFactorEnabled).HasColumnName("is_two_factor_enabled");
            HasRequired(playerAccount => playerAccount.PlayerPreferences).WithRequiredPrincipal();
            HasRequired(playerAccount => playerAccount.PlayerProfile).WithRequiredPrincipal();
            HasRequired(playerAccount => playerAccount.PlayerStats).WithRequiredPrincipal();
        }
    }
}
