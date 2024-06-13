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
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TornBot.Services.TornBotWeb.Services;
using TornBot.Services.TornStatsApi.Services;


namespace TornBot.Services.TornBotWeb
{
    public class TornBotWebModule : IModule
    {
        public TornBotWebModule()
        {
        }

        public IServiceCollection RegisterModule(IServiceCollection services)
        {
            Console.WriteLine("TornBotWebModule RegisterModule");

            services.AddControllers();
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen();
            
            services.AddAuthorization();

            services.AddSingleton<TornBotWebService>();
            services.AddHostedService(s => s.GetRequiredService<TornBotWebService>());

            return services;
        }
    }
}
