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

using Microsoft.EntityFrameworkCore;
using TornBot.Services.Database.Exceptions;
using TornBot.Services.Database.Migrations.Context;

namespace TornBot.Services.Database.Migrations.Dao;

public class MigrationsDao : BaseDao<MigrationsDbContext>, IMigrationsDao
{
    private readonly DbSet<Entities.Migrations> _dbSet;

    public MigrationsDao(MigrationsDbContext context) : base(context)
    {
        _dbSet = _context.Migrations;
    }

    /// <summary>
    /// Checks if a migration has already been applied, lookup via filename
    /// </summary>
    /// <param name="filename">Filename of migration</param>
    /// <returns>bool</returns>
    /// <exception cref="DatabaseException">Something went wrong and the inner exception has more details</exception>
    public bool HasBeenApplied(string filename)
    {
        try
        {
            List<Entities.Migrations> migration = _dbSet.Where(mig => mig.Name == filename).ToList();
            
            if (migration.Count > 0)
                return true;
            else
                return false;
        }
        catch (Exception e)
        {
            if (e.Message != "Table 'tornbot.Migrations' doesn't exist")
                throw;
            else
                throw new DatabaseException($"Error checking in migration has already been applied. Filename:{filename}", e);
        }
    }

    /// <summary>
    /// Adds a record that a migration file has been applied
    /// </summary>
    /// <param name="filename">Filename of migration</param>
    /// <param name="runAt">DateTime of migration</param>
    /// <exception cref="DatabaseException">Something went wrong and the inner exception has more details</exception>
    public void RecordMigrationApplied(string filename, DateTime runAt)
    {
        _dbSet.Add(new Entities.Migrations(filename, runAt));
        _context.SaveChanges();
    }
}