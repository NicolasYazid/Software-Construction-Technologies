using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities.PlayerPreferences;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the PlayerPreferences entity to the PlayerPreferences table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class PlayerPreferencesConfiguration : EntityTypeConfiguration<PlayerPreferences>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public PlayerPreferencesConfiguration()
        {
            ToTable("PlayerPreferences");
            HasKey(playerPreferences => playerPreferences.PlayerAccountId);
            Property(playerPreferences => playerPreferences.PlayerAccountId)
                .HasColumnName("player_account_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Property(playerPreferences => playerPreferences.LocaleId).HasColumnName("locale_id");
            Property(playerPreferences => playerPreferences.IsChatFilterEnabled).HasColumnName("is_chat_filter_enabled");
            HasRequired(playerPreferences => playerPreferences.Locale).WithMany().HasForeignKey(playerPreferences => playerPreferences.LocaleId);
        }
    }
}