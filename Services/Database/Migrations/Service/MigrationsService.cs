// TornBot
// 
// Copyright (C) 2024 TornBot.com
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
// 
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TornBot.Services.Database.Migrations.Dao;

namespace TornBot.Services.Database.Migrations.Service;

public class MigrationsService
{
    private readonly IServiceProvider _serviceProvider;

    public MigrationsService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void RunMigrations()
    {
        DatabaseContext dbContext =
            _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
        
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
                        if (HasBeenApplied(fileName))
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
                    RecordMigrationApplied(fileName, DateTime.UtcNow);
                    
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
    
    public bool HasBeenApplied(string filename)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dao = scope.ServiceProvider.GetRequiredService<IMigrationsDao>();

            return dao.HasBeenApplied(filename);
        }
    }
    
    public void RecordMigrationApplied(string filename, DateTime dateTime)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dao = scope.ServiceProvider.GetRequiredService<IMigrationsDao>();

            dao.RecordMigrationApplied(filename, dateTime);
        }
    }

}