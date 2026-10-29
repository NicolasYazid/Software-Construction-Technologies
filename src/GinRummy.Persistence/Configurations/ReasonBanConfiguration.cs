using System.Data.Entity.ModelConfiguration;

using GinRummy.Domain.Entities;

namespace GinRummy.Persistence.Configurations
{
    /// <summary>
    /// Maps the ReasonBan entity to the ReasonBan table of GinRummy_Dev. Entity Framework only reads and
    /// writes the table; the SQL scripts own its structure.
    /// </summary>
    internal class ReasonBanConfiguration : EntityTypeConfiguration<ReasonBan>
    {
        /// <summary>
        /// Declares the table, the key and the column of every property.
        /// </summary>
        public ReasonBanConfiguration()
        {
            ToTable("ReasonBan");
            HasKey(reasonBan => reasonBan.ReasonBanId);
            Property(reasonBan => reasonBan.ReasonBanId).HasColumnName("reason_ban_id");
            Property(reasonBan => reasonBan.Name)
                .HasColumnName("name")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(ReasonBan.NameMaxLength);
            Property(reasonBan => reasonBan.RestrictionScopeName)
                .HasColumnName("restriction_scope")
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(ReasonBan.RestrictionScopeNameMaxLength);
            Ignore(reasonBan => reasonBan.RestrictionScope);
            Ignore(reasonBan => reasonBan.IsChatRestricted);
            Ignore(reasonBan => reasonBan.IsAccountRestricted);
            HasMany(reasonBan => reasonBan.Translations)
                .WithRequired()
                .HasForeignKey(translation => translation.ReasonBanId);
        }
    }
}
