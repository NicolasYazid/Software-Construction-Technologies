using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

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
        // EF's automatic database initializer is disabled because GinRummy_Dev was created by
        // the team's own SQL scripts, not by Entity Framework.
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
        }
    }
}
