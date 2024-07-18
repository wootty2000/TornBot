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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using TornBot.Services.Database;
using TornBot.Services.Players.Database;
using TornBot.Services.Players.Database.Dao;
using TornBot.Services.Players.Service;

namespace TornBot.Services.Players
{
    public class PlayersModule : IModule
    {
        public IServiceCollection RegisterModule(IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {
                IConfigurationRoot config = serviceProvider.GetRequiredService<IConfigurationRoot>();
                DbContextFactory.ConfigureDbContext<PlayerStatusDbContext>(services, config);
                DbContextFactory.ConfigureDbContext<TornPlayerDbContext>(services, config);
            }

            services.AddScoped<IPlayerStatusDao, PlayerStatusDao>();
            services.AddScoped<ITornPlayerDao, TornPlayerDao>();
            services.AddScoped<IPlayerActivityImageService, PlayerActivityImageService>();
            services.AddScoped<PlayersService>();

            return services;
        }
    }
}
