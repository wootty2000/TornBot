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

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TornBot.Services.Discord.Interfaces;

namespace TornBot.Services.Discord.Services
{
    public sealed class DiscordService : IHostedService
    {
        private readonly IConfigurationRoot _config;
        private readonly ILogger<DiscordService> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly DiscordClient discord;
        private SlashCommandsExtension slashCommands;
        private static DiscordClient Client { get; set; }

        public DiscordService(IConfigurationRoot config, ILogger<DiscordService> logger, IHostApplicationLifetime applicationLifetime)
        {
            _config = config;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            discord = new(new()
            {
                Token = config.GetValue<string>("token"),
                TokenType = DSharpPlus.TokenType.Bot,
                Intents = DiscordIntents.All
            });
        }

        public async Task StartAsync(CancellationToken token)
        {
            IServiceProvider serviceProvider = TornBotApplication.GetIServiceProvider();
            
            DiscordActivity status = new("Torn - Dystopia", ActivityType.Watching);

            Assembly asm = Assembly.GetExecutingAssembly();

            //--------------------
            // Register event handlers
            var commandModuleType = typeof(IDiscordEventHandlerModule);
            var commandModules = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => commandModuleType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var module in commandModules)
            {
                var commandInstance = (IDiscordEventHandlerModule)ActivatorUtilities.CreateInstance(serviceProvider, module);
                commandInstance.RegisterEventHandlers(discord);
            }
            // End of registering event handlers
            //--------------------

            //--------------------
            // Text Commands - Setup
            // End of Text Commands
            //--------------------

            //--------------------
            // Slash Commands - Setup
            SlashCommandsConfiguration slashConfig = new()
            {
                Services = TornBotApplication.GetIServiceProvider()
            };
            slashCommands = discord.UseSlashCommands(slashConfig);

//#if RELEASE
//            slashCommands.RegisterCommands(asm);
//#else
            UInt64 guild = _config.GetValue<UInt64>("TestGuild");
            Console.WriteLine("guild: " + guild);

            Console.WriteLine("SlashCommands are registered in debug mode");
            slashCommands.RegisterCommands(asm, guild);
//#endif
            // End of Slash Commands
            //----------------------

            discord.GuildDownloadCompleted += GuildDownload;
            //slashCommands.SlashCommandErrored += EventListener.OnSlashCommandErrored;
            //slashCommands.AutocompleteErrored += EventListener.OnAutocompleteError;
            
            await discord.ConnectAsync(status, DSharpPlus.Entities.UserStatus.Online);
        }

        public async Task StopAsync(CancellationToken token)
        {
            await discord.DisconnectAsync();
            // More cleanup possibly here
        }

        public DiscordClient GetDiscordClient()
        {
            return discord;
        }

        private async Task GuildDownload(DiscordClient sender, GuildDownloadCompletedEventArgs args)
        {
        }
        public string GetStocksChannelId()
        {
            return _config.GetValue<string>("StocksChannelId");
        }
        public string GetInactivePlayerChannelId()
        {
            return _config.GetValue<string>("InactivePlayerChannelId");
        }
    }
}
