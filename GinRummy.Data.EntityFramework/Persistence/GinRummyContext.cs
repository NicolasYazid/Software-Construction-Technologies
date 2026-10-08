using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Persistence
{
    public class GinRummyContext : DbContext
    {
        // The schema of GinRummy_Dev belongs to the team's SQL scripts, so Entity Framework must never create or migrate it.
        static GinRummyContext()
        {
            Database.SetInitializer<GinRummyContext>(null);
        }

        public GinRummyContext(string connectionStringName)
            : base(connectionStringName)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }
        public DbSet<Locale> Locales { get; set; }
        public DbSet<PlayerStats> PlayerStats { get; set; }
        public DbSet<Rank> Ranks { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>().ToTable("Player");
            modelBuilder.Entity<Player>().HasKey(player => player.PlayerId);
            modelBuilder.Entity<Player>().Property(player => player.PlayerId)
                .HasColumnName("player_id");
            modelBuilder.Entity<Player>().Property(player => player.PublicTag)
                .HasColumnName("public_tag")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            modelBuilder.Entity<Player>().Property(player => player.Username)
                .HasColumnName("username");
            modelBuilder.Entity<Player>().Property(player => player.Email)
                .HasColumnName("email");
            modelBuilder.Entity<Player>().Property(player => player.PasswordHash)
                .HasColumnName("password");
            modelBuilder.Entity<Player>().Property(player => player.LocaleId)
                .HasColumnName("locale_id");
            modelBuilder.Entity<Player>().Property(player => player.CreatedAt)
                .HasColumnName("created_at");
            modelBuilder.Entity<Player>().Property(player => player.LastLoginAt)
                .HasColumnName("last_login_at");
            modelBuilder.Entity<Player>().Property(player => player.IsEmailVerified)
                .HasColumnName("is_email_verified");

            modelBuilder.Entity<VerificationCode>().ToTable("VerificationCode");
            modelBuilder.Entity<VerificationCode>().HasKey(verificationCode => verificationCode.VerificationCodeId);
            modelBuilder.Entity<VerificationCode>().Property(verificationCode => verificationCode.VerificationCodeId)
                .HasColumnName("verification_code_id");
            modelBuilder.Entity<VerificationCode>().Property(verificationCode => verificationCode.PlayerId)
                .HasColumnName("player_id");
            modelBuilder.Entity<VerificationCode>().Property(verificationCode => verificationCode.Purpose)
                .HasColumnName("purpose_id");
            modelBuilder.Entity<VerificationCode>().Property(verificationCode => verificationCode.CodeHash)
                .HasColumnName("code_hash");
            modelBuilder.Entity<VerificationCode>().Property(verificationCode => verificationCode.ExpiresAt)
                .HasColumnName("expires_at");
            modelBuilder.Entity<VerificationCode>().Property(verificationCode => verificationCode.UsedAt)
                .HasColumnName("used_at");
            modelBuilder.Entity<VerificationCode>().Property(verificationCode => verificationCode.Attempts)
                .HasColumnName("attempts");
            modelBuilder.Entity<VerificationCode>().Property(verificationCode => verificationCode.CreatedAt)
                .HasColumnName("created_at");

            modelBuilder.Entity<Locale>().ToTable("Locale");
            modelBuilder.Entity<Locale>().HasKey(locale => locale.LocaleId);
            modelBuilder.Entity<Locale>().Property(locale => locale.LocaleId).HasColumnName("locale_id");
            modelBuilder.Entity<Locale>().Property(locale => locale.LocaleCode).HasColumnName("locale_code");
            modelBuilder.Entity<Locale>().Property(locale => locale.DisplayName).HasColumnName("display_name");

            modelBuilder.Entity<PlayerStats>().ToTable("PlayerStats");
            modelBuilder.Entity<PlayerStats>().HasKey(stats => stats.PlayerId);
            modelBuilder.Entity<PlayerStats>().Property(stats => stats.PlayerId)
                .HasColumnName("player_id")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<PlayerStats>().Property(stats => stats.Wins).HasColumnName("wins");
            modelBuilder.Entity<PlayerStats>().Property(stats => stats.Losses).HasColumnName("losses");
            modelBuilder.Entity<PlayerStats>().Property(stats => stats.Score).HasColumnName("score");
            modelBuilder.Entity<PlayerStats>().Property(stats => stats.MatchesPlayed)
                .HasColumnName("matches_played")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            modelBuilder.Entity<PlayerStats>().HasRequired(stats => stats.Player)
                .WithOptional();

            modelBuilder.Entity<Rank>().ToTable("Rank");
            modelBuilder.Entity<Rank>().HasKey(rank => rank.RankId);
            modelBuilder.Entity<Rank>().Property(rank => rank.RankId).HasColumnName("rank_id");
            modelBuilder.Entity<Rank>().Property(rank => rank.Name).HasColumnName("name");
            modelBuilder.Entity<Rank>().Property(rank => rank.MinimumScore).HasColumnName("minimum_score");
            modelBuilder.Entity<Rank>().Property(rank => rank.MaximumScore).HasColumnName("maximum_score");
        }
    }
}
