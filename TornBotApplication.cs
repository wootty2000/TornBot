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
using TornBot.Services;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics;
using TornBot.Services.Database;

namespace TornBot
{
    public class TornBotApplication
    {
        private static IServiceProvider serviceProvider;
        private static WebApplication app;

        // Program entry point
        public static Task Main(string[] args) => new TornBotApplication().MainAsync();

        public async Task MainAsync()
        {
            IConfigurationRoot config = Config.LoadConfig();

            //We want a WebApplication as we will probably run a web site from here
            var builder = WebApplication.CreateBuilder();
            var services = builder.Services;

            // Add the configuration to the registered services
            services.AddSingleton(config);

            //Get the database setup
            DatabaseContext.Init(config, services);

            // RegisterModules will find all the IModule modules and call the module's RegisterModule function
            ModuleExtensions.RegisterModules(services); 

            serviceProvider = services.BuildServiceProvider();

            //Lets get it built
                app = builder.Build();
            
            await RunAsync(app);
        }

        public async Task RunAsync(WebApplication app)
        {
            app.RunAsync();

            //Delay forever
            await Task.Delay(-1);
        }

        public static IServiceProvider GetIServiceProvider() { return serviceProvider; }

        public static WebApplication GetWebApplication() { return app; }

        public static void LogError(string message, Exception exception)
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();
            var className = methodInfo.ReflectedType.Name;

            Console.WriteLine("ERROR: {0} - {1}\n\t{2}", className, message, exception);
        }
    }
}