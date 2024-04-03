using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TornBot.Database.Entities;
using static System.Net.Mime.MediaTypeNames;
using MySql.Data.MySqlClient;

namespace TornBot.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Settings> Settings { get; set; }
        public DbSet<TornPlayer> TornPlayers { get; set; }
        public DbSet<BattleStats> BattleStats { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public static void Init(IConfigurationRoot config, IServiceCollection services)
        {
            string connectionString = string.Format(
                "Server={0}; User ID={1}; Password={2}; Database={3}",
                config.GetValue<string>("DbHost"),
                config.GetValue<string>("DbUser"),
                config.GetValue<string>("DbPass"),
                config.GetValue<string>("DbDatabase")
            );
            
            services.AddDbContext<DatabaseContext>(options => options.UseMySQL(connectionString), ServiceLifetime.Transient);
        }
    }
}
