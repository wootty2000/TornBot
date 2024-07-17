//
// TornBot
//
// Copyright (C) 2024 TornBot.com
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TornBot.Services.Database.Entities;
using TornBot.Services.Players.Database.Entities;

namespace TornBot.Services.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Migrations> Migrations { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<TornPlayer> TornPlayers { get; set; }
        public DbSet<BattleStats> BattleStats { get; set; }
        public DbSet<ApiKeys> ApiKeys { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<ArmoryItems> ArmoryItems { get; set; }
        public DbSet<ArmoryItemRWBonus> ArmoryItemRWBonus { get; set; }
        public DbSet<LoadOuts> LoadOuts { get; set; }
        public DbSet<WeaponMods> WeaponMods { get; set; }

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
            
            services.AddDbContext<DatabaseContext>(
                options => options.UseMySQL(connectionString),
                ServiceLifetime.Transient
            );
        }

        public static void RunMigrations(IServiceProvider serviceProvider)
        {
            DatabaseContext dbContext =
                serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
            
            DbConnection conn = dbContext.Database.GetDbConnection(); // Get Database connection
            ConnectionState initialConnectionState = conn.State;

            try
            {
                if (initialConnectionState != ConnectionState.Open)
                    conn.Open(); // open connection if not already open

                using (DbCommand cmd = conn.CreateCommand())
                {
                    string[] fileEntries = Directory.GetFiles("sql");
                    foreach (string fileName in fileEntries)
                    {
                        //We are only interested in SQL files
                        if (!fileName.EndsWith(".sql"))
                            continue;

                        try
                        {
                            //Skip any files that have already been applied
                            var migration = dbContext.Migrations.Where(mig => mig.Name == fileName).ToList();
                            if (migration.Count > 0)
                                continue;
                        }
                        catch (Exception e)
                        {
                            //If the initial db create has not run yet, the check will fail. Just ignore as it will create
                            //the migrations table first
                            if (e.Message != "Table 'tornbot.Migrations' doesn't exist")
                                throw;
                        }
                        
                        var sql = System.IO.File.ReadAllText(fileName);
                        string[] commands = sql.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
                        
                        // Iterate the string array and execute each one.
                        foreach (string command in commands)
                        {
                            cmd.CommandText = command;
                            cmd.ExecuteNonQuery();
                        }

                        //Add to the migrations table so we dont run it again
                        dbContext.Migrations.Add(new Migrations(fileName, DateTime.UtcNow));
                        dbContext.SaveChanges();
                    }
                }
            }
            finally
            {
                if (initialConnectionState != ConnectionState.Open)
                    conn.Close(); // only close connection if not initially open
            }
        }
    }
}
