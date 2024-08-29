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

using Microsoft.Extensions.DependencyInjection;
using TornBot.Services.Database.Settings.Dao;

namespace TornBot.Services.Database.Settings.Service;

public class SettingsService
{
    private readonly IServiceProvider _serviceProvider;

    public SettingsService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void SetSetting(string module, string name, string value)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dao = scope.ServiceProvider.GetRequiredService<ISettingsDao>();

            dao.SetSetting(module, name, value);
        }
    }

    public string GetSetting(string module, string name)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dao = scope.ServiceProvider.GetRequiredService<ISettingsDao>();

            return dao.GetSetting(module, name);
        }
    }



}