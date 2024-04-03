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
            options.UseNpgsql(Configuration.GetConnectionString("WebApiDatabase"));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new ConfigConfiguration());
            modelBuilder.ApplyConfiguration(new UserSettingConfiguration());
        }
        public DbSet<User>? Users { get; set; }
        public DbSet<UserSetting>? UserSettings { get; set; }
        public DbSet<Config>? Configs { get; set; }
    }

}

