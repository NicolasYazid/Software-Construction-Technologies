using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Entities;

namespace GinRummy.Data.EntityFramework.Persistence
{
    /// <summary>
    /// Entity Framework 6 context for GinRummy_Dev. Maps the Domain's POCO entities
    /// against the tables the SQL scripts already created; EF never creates or
    /// migrates the schema.
    /// </summary>
    public class GinRummyContext : DbContext
    {
        /// <summary>
        /// Disables EF's automatic database initializer, because GinRummy_Dev was
        /// created by the team's own SQL scripts, not by Entity Framework.
        /// </summary>
        static GinRummyContext()
        {
            Database.SetInitializer<GinRummyContext>(null);
        }

        /// <summary>
        /// Opens the context against the connection string of the given name, read
        /// from the calling application's configuration file.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        public GinRummyContext(string connectionStringName)
            : base(connectionStringName)
        {
        }

        /// <summary>
        /// Gets or sets the Player set.
        /// </summary>
        public DbSet<Player> Players { get; set; }

        /// <summary>
        /// Gets or sets the VerificationCode set.
        /// </summary>
        public DbSet<VerificationCode> VerificationCodes { get; set; }

        /// <summary>
        /// Gets or sets the Locale set.
        /// </summary>
        public DbSet<Locale> Locales { get; set; }

        /// <summary>
        /// Gets or sets the PlayerStats set.
        /// </summary>
        public DbSet<PlayerStats> PlayerStats { get; set; }

        /// <summary>
        /// Gets or sets the Rank set.
        /// </summary>
        public DbSet<Rank> Ranks { get; set; }

        /// <summary>
        /// Maps the Domain entities to the columns GinRummy_Dev already defines.
        /// </summary>
        /// <param name="modelBuilder">Builder EF uses to configure the model.</param>
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
                .WithMany()
                .HasForeignKey(stats => stats.PlayerId);

            modelBuilder.Entity<Rank>().ToTable("Rank");
            modelBuilder.Entity<Rank>().HasKey(rank => rank.RankId);
            modelBuilder.Entity<Rank>().Property(rank => rank.RankId).HasColumnName("rank_id");
            modelBuilder.Entity<Rank>().Property(rank => rank.Name).HasColumnName("name");
            modelBuilder.Entity<Rank>().Property(rank => rank.MinimumScore).HasColumnName("minimum_score");
            modelBuilder.Entity<Rank>().Property(rank => rank.MaximumScore).HasColumnName("maximum_score");
        }
    }
}
