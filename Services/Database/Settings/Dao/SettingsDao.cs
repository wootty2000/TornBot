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
using TornBot.Services.Database.Settings.Context;

namespace TornBot.Services.Database.Settings.Dao;

public class SettingsDao : BaseDao<SettingsDbContext>, ISettingsDao
{
    private readonly DbSet<Entities.Settings> _dbSet;

    public SettingsDao(SettingsDbContext context) : base(context)
    {
        _dbSet = _context.Settings;
    }

    /// <summary>
    /// Attempts to set a setting in the database
    /// If there is already an existing setting for the module and name, it will be updated, otherwise a new entry is added
    /// </summary>
    /// <param name="module">Module name</param>
    /// <param name="name">Setting name</param>
    /// <param name="value">Setting value</param>
    /// <exception cref="DatabaseException">Something went wrong and the inner exception has more details</exception>
    public void SetSetting(string module, string name, string value)
    {
        try
        {
            Entities.Settings? setting = _dbSet.FirstOrDefault(s => s.Module == module && s.Name == name);

            if (setting == null)
            {
                Entities.Settings newSetting = new Entities.Settings
                {
                    Module = module,
                    Name = name,
                    Value = value
                };
                _dbSet.Add(newSetting);
                _context.SaveChanges();
            }
            else
            {
                setting.Value = value;
                _dbSet.Update(setting);
                _context.SaveChanges();
            }
        }
        catch (Exception e)
        {
            throw new DatabaseException($"Error settings Setting record. Module:{module} Name:{name} Value:{value}", e);
        }
    }

    /// <summary>
    /// Attempts to get a setting from the database
    /// </summary>
    /// <param name="module">Module name</param>
    /// <param name="name">Setting name</param>
    /// <returns>string</returns>
    /// <exception cref="DatabaseException">Something went wrong and the inner exception has more details</exception>
    public string GetSetting(string module, string name)
    {
        try
        {
            Entities.Settings? setting = _dbSet.FirstOrDefault(s => s.Module == module && s.Name == name);

            return setting != null ? setting.Value : "";
        }
        catch (Exception e)
        {
            throw new DatabaseException($"Error getting Setting record. Module:{module} Name:{name}", e);
        }
    }
}