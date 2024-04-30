using BidenSurfer.Infras.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using S5E.ABPCMS.Infrastructure.EntityConfigurations;

namespace BidenSurfer.Infras.Domains
{
    public class AppDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public AppDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to postgres with connection string from app settings
            var connectionString = Configuration.GetConnectionString("WebApiDatabase");
            options.UseNpgsql(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UserSettingConfiguration());
            modelBuilder.ApplyConfiguration(new ConfigConfiguration());
            modelBuilder.ApplyConfiguration(new ScannerConfiguration());
            modelBuilder.ApplyConfiguration(new ScannerSettingConfiguration());
        }
        public DbSet<User>? Users { get; set; }
        public DbSet<UserSetting>? UserSettings { get; set; }
        public DbSet<Config>? Configs { get; set; }
        public DbSet<Scanner>? Scanners { get; set; }
        public DbSet<ScannerSetting>? ScannerSetting { get; set; }
    }

}

