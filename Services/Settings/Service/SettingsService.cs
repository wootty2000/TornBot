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

using Microsoft.Extensions.Logging;

namespace TornBot.Services.Settings.Service;

public class SettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly Services.Database.Settings.Service.SettingsService _settingsService;
    private string _moduleName;
        
    public SettingsService(
        ILogger<SettingsService> logger,
        Services.Database.Settings.Service.SettingsService settingsService
    )
    {
        _logger = logger;
        _settingsService = settingsService;
    }

    public void SetModuleName(string moduleName)
    {
        _moduleName = moduleName;
    }

    /// <summary>
    /// Attempts to set a setting in the database via the DAO
    /// If there is already an existing setting for the module and name, it will be updated, otherwise a new entry is added
    /// </summary>
    /// <param name="name">Setting name</param>
    /// <param name="value">Setting value</param>
    /// <exception cref="DatabaseException">Something went wrong and the inner exception has more details</exception>
    public void SetSetting(string name, string value)
    {
        try
        {
            _settingsService.SetSetting(_moduleName, name, value);
        }
        catch (Exception e)
        {
            _logger.LogError(e, string.Format("Error setting Setting to DB. Module:{0} Name:{1} Value:{2}", _moduleName, name, value));
            throw;
        }
    }
    
    /// <summary>
    /// Attempts to get a setting from the database via the DAO
    /// </summary>
    /// <param name="name">Setting name</param>
    /// <returns>string</returns>
    /// <exception cref="DatabaseException">Something went wrong and the inner exception has more details</exception>
    public string GetSetting(string name)
    {
        try
        {
            return _settingsService.GetSetting(_moduleName, name);
        }
        catch (Exception e)
        {
            _logger.LogError(e, string.Format("Error getting Setting from DB. Module:{0} Name:{1}", _moduleName, name));
            throw;
        }
    }
}