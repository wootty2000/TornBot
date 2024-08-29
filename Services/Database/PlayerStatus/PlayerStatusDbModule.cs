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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TornBot.Services.Database.PlayerStatus.Context;
using TornBot.Services.Database.PlayerStatus.Dao;
using TornBot.Services.Database.PlayerStatus.Service;

namespace TornBot.Services.Database.PlayerStatus;

public class PlayerStatusDbModule : IDbModule
{
    public void RegisterDbModule(IConfigurationRoot config, IServiceCollection services)
    {
        DbContextFactory.ConfigureDbContext<PlayerStatusDbContext>(services, config);
        
        services.AddTransient<IPlayerStatusDao, PlayerStatusDao>();
        services.AddScoped<PlayerStatusService>();

    }
}