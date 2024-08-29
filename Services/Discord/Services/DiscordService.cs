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
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using TornBot.Services.Discord.Interfaces;
using TornBot.Services.Logger;
using TornBot.Services.Logger.Targets;
using TornBot.Services.Settings.Service;

namespace TornBot.Services.Discord.Services
{
    public sealed class DiscordService : IHostedService
    {
        private readonly ILogger<DiscordService> _logger;
        private readonly LoggingConfiguration _nlogConfig;
        private readonly SettingsDiscordService _settingsDiscordService;
        private readonly DiscordClient _discord;
        private SlashCommandsExtension _slashCommands;

        public DiscordService(
            ILogger<DiscordService> logger, 
            LoggingConfiguration nlogConfig,
            SettingsDiscordService settingsDiscordService
        )
        {
            _logger = logger;
            _nlogConfig = nlogConfig;

            _settingsDiscordService = settingsDiscordService;
            string discordToken = settingsDiscordService.GetToken();

            if (string.IsNullOrWhiteSpace(discordToken))
            {
                logger.LogError("Failed to create DiscordClient as Token has not been set in the database");
                _discord = null;
            }
            else
            {
                _discord = new(new()
                {
                    Token = discordToken,
                    TokenType = DSharpPlus.TokenType.Bot,
                    Intents = DiscordIntents.All,
                    LoggerFactory = new NLogLoggerFactory()              
                });
            }
        }

        public async Task StartAsync(CancellationToken token)
        {
            if (_discord == null)
                return;
            
            IServiceProvider serviceProvider = TornBotApplication.GetIServiceProvider();

            string watchingName = _settingsDiscordService.GetFactionName();

            DiscordActivity discordActivity = null;
            if (!string.IsNullOrWhiteSpace(watchingName))
            {
                discordActivity = new($"Torn - {watchingName}", ActivityType.Watching);
            }

            Assembly asm = Assembly.GetExecutingAssembly();

            //--------------------
            // Register event handlers
            var commandModuleType = typeof(IDiscordEventHandlerModule);
            var commandModules = 
                Assembly.GetExecutingAssembly().GetTypes()
                .Where(
                    t => commandModuleType.IsAssignableFrom(t) && 
                    !t.IsInterface &&
                    !t.IsAbstract
                );

            foreach (var module in commandModules)
            {
                var commandInstance = (IDiscordEventHandlerModule)ActivatorUtilities.CreateInstance(serviceProvider, module);
                commandInstance.RegisterEventHandlers(_discord);
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
            _slashCommands = _discord.UseSlashCommands(slashConfig);
            
            try
            {
                string guildId = _settingsDiscordService.GetGuildId();
                if (string.IsNullOrWhiteSpace(guildId))
                    _logger.LogError("Error registering Discord slash commands as guildId has not been set in the database");
                else
                {
                    if (!UInt64.TryParse(guildId, out UInt64 guildIdUInt))
                        _logger.LogError("GuildId value in database is not valid");
                    else
                        _slashCommands.RegisterCommands(asm, guildIdUInt);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error registering Discord slash command. Check inner exception");
            }
            
            
            // End of Slash Commands
            //----------------------

            _discord.GuildDownloadCompleted += GuildDownload;
            //slashCommands.SlashCommandErrored += EventListener.OnSlashCommandErrored;
            //slashCommands.AutocompleteErrored += EventListener.OnAutocompleteError;
            
            await _discord.ConnectAsync(discordActivity, DSharpPlus.Entities.UserStatus.Online);
        }

        public async Task StopAsync(CancellationToken token)
        {
            await _discord.DisconnectAsync();
            // More cleanup possibly here
        }

        public DiscordClient GetDiscordClient()
        {
            return _discord;
        }

        private async Task GuildDownload(DiscordClient sender, GuildDownloadCompletedEventArgs args)
        {
            try
            {
                string logChannelId = _settingsDiscordService.GetLogChannelId();
                

                if (string.IsNullOrEmpty(logChannelId))
                    _logger.LogError("Error initialising logging to Discord as LogChannelId has not been set in the database");
                else
                {
                    if (!UInt64.TryParse(logChannelId, out UInt64 logChannelIdUInt))
                        _logger.LogError("LogChannelId value in database is not valid");
                    else
                    {
                        var logChannel = await _discord.GetChannelAsync(logChannelIdUInt);
                        
                        // Initialize the Discord target with the connected client and log channel
                        InitializeDiscordTarget(_discord, logChannel);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error initialising logging to Discord. See inner exception");
            }
        }
        
        private void InitializeDiscordTarget(DiscordClient discordClient, DiscordChannel logChannel)
        {
            var discordTarget = _nlogConfig.FindTargetByName<DiscordTarget>("discord");
            if (discordTarget != null)
            {
                discordTarget.Initialize(discordClient, logChannel);
                LogManager.Configuration = _nlogConfig;
                LogManager.ReconfigExistingLoggers();
            }
        }
        
        public string GetStocksChannelId()
        {
            return _settingsDiscordService.GetStockChannelId();
        }

        public string GetInactivePlayerChannelId()
        {
            return _settingsDiscordService.GetInactiveChannelId();
        }
    }
}
