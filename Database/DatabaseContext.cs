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
        public DbSet<Stats> Stats { get; set; }

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

            var provider = "Sqlite";

            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            String DbPath = System.IO.Path.Join(path, "tornbot.db");

            services.AddDbContext<DatabaseContext>(options => options.UseMySQL(connectionString), ServiceLifetime.Transient);
            //services.AddDbContext<DatabaseContext>(options => options.UseSqlite($"Data Source={DbPath}"));

            /*services.AddDbContext<DatabaseContext>(
                options => _ = provider switch
                {
                    "Sqlite" => options.UseSqlite(
                        $"Data Source={DbPath}",
                        x => x.MigrationsAssembly("SqliteMigrations")),

                    "MySql" => options.UseMySQL(
                        connectionString,
                        x => x.MigrationsAssembly("SqlServerMigrations")),

                    _ => throw new Exception($"Unsupported provider: {provider}")
                });
            */
        }
    }
}
