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
using TornBot.Services.Database.TornFactions.Context;
using TornBot.Services.Database.TornFactions.Dao;
using TornBot.Services.Database.TornFactions.Service;

namespace TornBot.Services.Database.Factions;

public class TornFactionsDbModule : IDbModule
{
    public void RegisterDbModule(IConfigurationRoot config, IServiceCollection services)
    {
        DbContextFactory.ConfigureDbContext<TornFactionsDbContext>(services, config);
        
        services.AddTransient<ITornFactionsDao, TornFactionsDao>();
        services.AddScoped<TornFactionsService>();
    }
}