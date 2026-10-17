using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Data.EntityFramework.Configurations;
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
        /// from the calling application's configuration file. Related entities are loaded
        /// only when a query asks for them with Include.
        /// </summary>
        /// <param name="connectionStringName">Name of the entry in App.config.</param>
        public GinRummyContext(string connectionStringName)
            : base(connectionStringName)
        {
            // Navigation properties load only through Include. Lazy loading would run queries after
            // the unit of work disposed the context, and a proxy is a type WCF cannot serialize.
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        /// <summary>
        /// Gets or sets the Player set.
        /// </summary>
        public DbSet<Player> Players { get; set; }

        /// <summary>
        /// Gets or sets the PlayerAccount set.
        /// </summary>
        public DbSet<PlayerAccount> PlayerAccounts { get; set; }

        /// <summary>
        /// Gets or sets the PlayerProfile set.
        /// </summary>
        public DbSet<PlayerProfile> PlayerProfiles { get; set; }

        /// <summary>
        /// Gets or sets the PlayerSession set.
        /// </summary>
        public DbSet<PlayerSession> PlayerSessions { get; set; }

        /// <summary>
        /// Gets or sets the PlayerStats set.
        /// </summary>
        public DbSet<PlayerStats> PlayerStats { get; set; }

        /// <summary>
        /// Gets or sets the SocialMedia set.
        /// </summary>
        public DbSet<SocialMedia> SocialMedias { get; set; }

        /// <summary>
        /// Gets or sets the VerificationCode set.
        /// </summary>
        public DbSet<VerificationCode> VerificationCodes { get; set; }

        /// <summary>
        /// Gets or sets the Email set.
        /// </summary>
        public DbSet<Email> Emails { get; set; }

        /// <summary>
        /// Gets or sets the FriendRequest set.
        /// </summary>
        public DbSet<FriendRequest> FriendRequests { get; set; }

        /// <summary>
        /// Gets or sets the FriendShip set.
        /// </summary>
        public DbSet<FriendShip> FriendShips { get; set; }

        /// <summary>
        /// Gets or sets the Report set.
        /// </summary>
        public DbSet<Report> Reports { get; set; }

        /// <summary>
        /// Gets or sets the Warning set.
        /// </summary>
        public DbSet<Warning> Warnings { get; set; }

        /// <summary>
        /// Gets or sets the Ban set.
        /// </summary>
        public DbSet<Ban> Bans { get; set; }

        /// <summary>
        /// Gets or sets the Message set.
        /// </summary>
        public DbSet<Message> Messages { get; set; }

        /// <summary>
        /// Gets or sets the ProhibitedWord set.
        /// </summary>
        public DbSet<ProhibitedWord> ProhibitedWords { get; set; }

        /// <summary>
        /// Gets or sets the Locale set.
        /// </summary>
        public DbSet<Locale> Locales { get; set; }

        /// <summary>
        /// Gets or sets the Platform set.
        /// </summary>
        public DbSet<Platform> Platforms { get; set; }

        /// <summary>
        /// Gets or sets the Rank set.
        /// </summary>
        public DbSet<Rank> Ranks { get; set; }

        /// <summary>
        /// Gets or sets the ReportCategory set.
        /// </summary>
        public DbSet<ReportCategory> ReportCategories { get; set; }

        /// <summary>
        /// Gets or sets the ReportStatus set.
        /// </summary>
        public DbSet<ReportStatus> ReportStatuses { get; set; }

        /// <summary>
        /// Gets or sets the ReasonBan set.
        /// </summary>
        public DbSet<ReasonBan> ReasonBans { get; set; }

        /// <summary>
        /// Gets or sets the GuestSession set.
        /// </summary>
        public DbSet<GuestSession> GuestSessions { get; set; }

        /// <summary>
        /// Gets or sets the GameSession set.
        /// </summary>
        public DbSet<GameSession> GameSessions { get; set; }

        /// <summary>
        /// Gets or sets the GameParticipant set.
        /// </summary>
        public DbSet<GameParticipant> GameParticipants { get; set; }

        /// <summary>
        /// Gets or sets the Challenge set.
        /// </summary>
        public DbSet<Challenge> Challenges { get; set; }

        /// <summary>
        /// Gets or sets the GameHistory set.
        /// </summary>
        public DbSet<GameHistory> GameHistories { get; set; }

        /// <summary>
        /// Registers one configuration per entity. The translation tables (RankLocale,
        /// ReportCategoryLocale, ReasonBanLocale and ProhibitedWordLocale) have no set of
        /// their own because they are always reached through their parent.
        /// </summary>
        /// <param name="modelBuilder">Builder EF uses to configure the model.</param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new BanConfiguration());
            modelBuilder.Configurations.Add(new ChallengeConfiguration());
            modelBuilder.Configurations.Add(new EmailConfiguration());
            modelBuilder.Configurations.Add(new FriendRequestConfiguration());
            modelBuilder.Configurations.Add(new FriendShipConfiguration());
            modelBuilder.Configurations.Add(new GameHistoryConfiguration());
            modelBuilder.Configurations.Add(new GameParticipantConfiguration());
            modelBuilder.Configurations.Add(new GameSessionConfiguration());
            modelBuilder.Configurations.Add(new GuestSessionConfiguration());
            modelBuilder.Configurations.Add(new LocaleConfiguration());
            modelBuilder.Configurations.Add(new MessageConfiguration());
            modelBuilder.Configurations.Add(new PlatformConfiguration());
            modelBuilder.Configurations.Add(new PlayerConfiguration());
            modelBuilder.Configurations.Add(new PlayerAccountConfiguration());
            modelBuilder.Configurations.Add(new PlayerProfileConfiguration());
            modelBuilder.Configurations.Add(new PlayerSessionConfiguration());
            modelBuilder.Configurations.Add(new PlayerStatsConfiguration());
            modelBuilder.Configurations.Add(new ProhibitedWordConfiguration());
            modelBuilder.Configurations.Add(new ProhibitedWordLocaleConfiguration());
            modelBuilder.Configurations.Add(new RankConfiguration());
            modelBuilder.Configurations.Add(new RankLocaleConfiguration());
            modelBuilder.Configurations.Add(new ReasonBanConfiguration());
            modelBuilder.Configurations.Add(new ReasonBanLocaleConfiguration());
            modelBuilder.Configurations.Add(new ReportConfiguration());
            modelBuilder.Configurations.Add(new ReportCategoryConfiguration());
            modelBuilder.Configurations.Add(new ReportCategoryLocaleConfiguration());
            modelBuilder.Configurations.Add(new ReportStatusConfiguration());
            modelBuilder.Configurations.Add(new SocialMediaConfiguration());
            modelBuilder.Configurations.Add(new VerificationCodeConfiguration());
            modelBuilder.Configurations.Add(new WarningConfiguration());
        }
    }
}
