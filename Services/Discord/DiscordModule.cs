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

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TornBot.Services.Discord.Services;

namespace TornBot.Services.Discord
{
    public class DiscordModule : IModule
    {
        public DiscordModule()
        {
        }

        public IServiceCollection RegisterModule(IServiceCollection services)
        {
            services.AddSingleton<DiscordService>();
            services.AddHostedService(s => s.GetRequiredService<DiscordService>());
            services.AddSingleton(s => s.GetRequiredService<DiscordService>().GetDiscordClient());

            return services;
        }
    }
}
