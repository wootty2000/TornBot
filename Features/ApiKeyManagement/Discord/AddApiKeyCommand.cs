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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using TornBot.Exceptions;
using TornBot.Services.Discord.Interfaces;
using TornBot.Services.TornApi.Services;
using TornBot.Services.TornStatsApi.Services;

namespace TornBot.Features.ApiKeyManagement.Discord
{
    public class AddApiKeyCommand : ApplicationCommandModule, IDiscordEventHandlerModule
    {
        private TornApiService _tornApiService;
        private TornStatsApiService _tornStatsApiService;
        
        public AddApiKeyCommand(TornApiService tornApiService, TornStatsApiService tornStatsApiService)
        {
            _tornApiService = tornApiService;
            _tornStatsApiService = tornStatsApiService;
        }
        
        public void RegisterEventHandlers(DiscordClient client)
        {
            client.ModalSubmitted += ModalEventHandler;
        }

        [SlashCommand("AddApiKey", "Adds Api Keys to the system")]
        public async Task AddApiKey(InteractionContext ctx)
        {
            DiscordInteractionResponseBuilder addApiKeyModal = new DiscordInteractionResponseBuilder()
                .WithTitle("Add Api Keys")
                .WithCustomId("AddApiKey-AddApiKeyModal")
                .AddComponents(new TextInputComponent("Torn Api Key", "TornApiKey", "Torn Api Key (Mandatory)", null, true))
                .AddComponents(new TextInputComponent("TornStats Api Key", "TornStatsApiKey", "TornStats Api Key (Optional)", null, false));
        
            await ctx.CreateResponseAsync(InteractionResponseType.Modal, addApiKeyModal);
        }

        private Task ModalEventHandler(DiscordClient sender, ModalSubmitEventArgs args)
        {
            if (args.Interaction.Type == InteractionType.ModalSubmit &&
                args.Interaction.Data.CustomId == "AddApiKey-AddApiKeyModal")
            {
                string responseMessage;
                
                try
                {
                    _tornApiService.AddApiKey(args.Values["TornApiKey"].Trim());
                    
                    responseMessage = "Torn API key added to the database";
                }
                catch (ApiCallFailureException e)
                {
                    responseMessage = e.InnerException switch
                    {
                        InvalidKeyException => "Supplied API key is invalid",
                        ApiNotAvailableException => "Torn API is currently unavailable. Please try later",
                        ApiKeyOwnerInactiveException => "Supplied API key unavailable due to player inactivity",
                        _ => "Unknown error adding API key"
                    };
                }
                catch (Exception)
                {
                    responseMessage = "Unknown error adding API key";
                }
                
                
                if (args.Values["TornStatsApiKey"].Trim().Length > 5)
                {
                    responseMessage += "\n";
                    
                    try
                    {
                        _tornStatsApiService.AddApiKey(args.Values["TornStatsApiKey"].Trim());
                        
                        responseMessage += "TornStats API key added to the database";

                    }
                    catch (ApiCallFailureException e)
                    {
                        responseMessage += e.InnerException switch
                        {
                            InvalidKeyException => "Supplied TornStats API key is invalid",
                            ApiNotAvailableException => "TornStats API is currently unavailable. Please try later",
                            _ => "Unknown error adding TornStats API key"
                        };
                    }
                    catch (Exception)
                    {
                        responseMessage += "Unknown error adding TornStats API key";
                    }
                }
                
                args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent(responseMessage).AsEphemeral()).Wait();
            }
            
            return Task.CompletedTask;
        }
    }
}
