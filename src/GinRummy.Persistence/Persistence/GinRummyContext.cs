using System.Data.Entity;

using GinRummy.Domain.Entities;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.ChatMessaging;
using GinRummy.Domain.Entities.Friendship;
using GinRummy.Domain.Entities.Game;
using GinRummy.Domain.Entities.PlayerModeration;
using GinRummy.Domain.Entities.PlayerRanking;
using GinRummy.Domain.Entities.Players;
using GinRummy.Persistence.Configurations;

namespace GinRummy.Persistence.Persistence
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
            // Navigation properties load only through Include.
            // Lazy loading would run queries after the unit of work disposed the context, and a proxy is a type WCF cannot serialize.
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        /// <summary>
        /// Gets the Player set.
        /// </summary>
        public DbSet<Player> Players
        {
            get { return Set<Player>(); }
        }

        /// <summary>
        /// Gets the PlayerAccount set.
        /// </summary>
        public DbSet<PlayerAccount> PlayerAccounts
        {
            get { return Set<PlayerAccount>(); }
        }

        /// <summary>
        /// Gets the PlayerProfile set.
        /// </summary>
        public DbSet<PlayerProfile> PlayerProfiles
        {
            get { return Set<PlayerProfile>(); }
        }

        /// <summary>
        /// Gets the PlayerSession set.
        /// </summary>
        public DbSet<PlayerSession> PlayerSessions
        {
            get { return Set<PlayerSession>(); }
        }

        /// <summary>
        /// Gets the PlayerStats set.
        /// </summary>
        public DbSet<PlayerStats> PlayerStats
        {
            get { return Set<PlayerStats>(); }
        }

        /// <summary>
        /// Gets the SocialMedia set.
        /// </summary>
        public DbSet<SocialMedia> SocialMedias
        {
            get { return Set<SocialMedia>(); }
        }

        /// <summary>
        /// Gets the VerificationCode set.
        /// </summary>
        public DbSet<VerificationCode> VerificationCodes
        {
            get { return Set<VerificationCode>(); }
        }

        /// <summary>
        /// Gets the Email set.
        /// </summary>
        public DbSet<Email> Emails
        {
            get { return Set<Email>(); }
        }

        /// <summary>
        /// Gets the FriendRequest set.
        /// </summary>
        public DbSet<FriendRequest> FriendRequests
        {
            get { return Set<FriendRequest>(); }
        }

        /// <summary>
        /// Gets the FriendShip set.
        /// </summary>
        public DbSet<FriendShip> FriendShips
        {
            get { return Set<FriendShip>(); }
        }

        /// <summary>
        /// Gets the Warning set.
        /// </summary>
        public DbSet<Warning> Warnings
        {
            get { return Set<Warning>(); }
        }

        /// <summary>
        /// Gets the Ban set.
        /// </summary>
        public DbSet<Ban> Bans
        {
            get { return Set<Ban>(); }
        }

        /// <summary>
        /// Gets the Message set.
        /// </summary>
        public DbSet<Message> Messages
        {
            get { return Set<Message>(); }
        }

        /// <summary>
        /// Gets the ProhibitedWord set.
        /// </summary>
        public DbSet<ProhibitedWord> ProhibitedWords
        {
            get { return Set<ProhibitedWord>(); }
        }

        /// <summary>
        /// Gets the Locale set.
        /// </summary>
        public DbSet<Locale> Locales
        {
            get { return Set<Locale>(); }
        }

        /// <summary>
        /// Gets the Platform set.
        /// </summary>
        public DbSet<Platform> Platforms
        {
            get { return Set<Platform>(); }
        }

        /// <summary>
        /// Gets the Rank set.
        /// </summary>
        public DbSet<Rank> Ranks
        {
            get { return Set<Rank>(); }
        }

        /// <summary>
        /// Gets the ReasonBan set.
        /// </summary>
        public DbSet<ReasonBan> ReasonBans
        {
            get { return Set<ReasonBan>(); }
        }

        /// <summary>
        /// Gets the GuestSession set.
        /// </summary>
        public DbSet<GuestSession> GuestSessions
        {
            get { return Set<GuestSession>(); }
        }

        /// <summary>
        /// Gets the GameSession set.
        /// </summary>
        public DbSet<GameSession> GameSessions
        {
            get { return Set<GameSession>(); }
        }

        /// <summary>
        /// Gets the GameParticipant set.
        /// </summary>
        public DbSet<GameParticipant> GameParticipants
        {
            get { return Set<GameParticipant>(); }
        }

        /// <summary>
        /// Gets the Challenge set.
        /// </summary>
        public DbSet<Challenge> Challenges
        {
            get { return Set<Challenge>(); }
        }

        /// <summary>
        /// Gets the GameHistory set.
        /// </summary>
        public DbSet<GameHistory> GameHistories
        {
            get { return Set<GameHistory>(); }
        }

        /// <summary>
        /// Registers one configuration per entity. The translation tables (RankLocale,
        /// ReasonBanLocale and ProhibitedWordLocale) have no set of
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
            modelBuilder.Configurations.Add(new SocialMediaConfiguration());
            modelBuilder.Configurations.Add(new VerificationCodeConfiguration());
            modelBuilder.Configurations.Add(new WarningConfiguration());
        }
    }
}
