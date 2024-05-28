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
using DSharpPlus.SlashCommands;
using TornBot.Exceptions;
using TornBot.Services.TornApi.Services;
using TornBot.Services.TornStatsApi.Services;

namespace TornBot.Features.ApiKeyManagement.Discord
{
    public class ApiKeyCommand : ApplicationCommandModule
    {
        private TornApiService _tornApiService;
        private TornStatsApiService _tornStatsApiService;
        
        public ApiKeyCommand(TornApiService tornApiService, TornStatsApiService tornStatsApiService)
        {
            _tornApiService = tornApiService;
            _tornStatsApiService = tornStatsApiService;
        }
        
        
        [SlashCommand("addapi", "Add api key")]
        public async Task ApiKey(
            InteractionContext ctx,
            //[ChoiceProvider(typeof(ChoiceProvider))]
            [Option("apikey", "Add torn api key with public access level")] string api_key,
            [Option("TornStatsApiKey", "Add torn stats api key - not required (you can input 0)")] string tornStats_api_key
        )
        {
            await ctx.DeferAsync(true);

            string responseMessage;

            if (api_key.Trim().Length > 5)
            {
                try
                {
                    _tornApiService.AddApiKey(api_key.Trim());
                    responseMessage = "Torn API key added to the database\n";
                }
                catch (ApiCallFailureException e)
                {
                    responseMessage = e.InnerException switch
                    {
                        InvalidKeyException => "Supplied API key is invalid\n",
                        ApiNotAvailableException => "Torn API is currently unavailable. Please try later\n",
                        ApiKeyOwnerInactiveException => "Supplied API key unavailable due to player inactivity\n",
                        _ => "Unknown error adding API key\n"
                    };
                }
                catch (Exception)
                {
                    responseMessage = "Unknown error adding API key\n";
                }
            }
            else
            {
                responseMessage = "";
            }

            if (tornStats_api_key.Trim().Length > 5)
            {
                try
                {
                    _tornStatsApiService.AddApiKey(api_key.Trim());
                }
                catch (ApiCallFailureException e)
                {
                    responseMessage += e.InnerException switch
                    {
                        InvalidKeyException => "Supplied API key is invalid",
                        ApiNotAvailableException => "Torn API is currently unavailable. Please try later",
                        ApiKeyOwnerInactiveException => "Supplied API key unavailable due to player inactivity",
                        _ => "Unknown error adding API key"
                    };
                }
                catch (Exception)
                {
                    responseMessage += "Unknown error adding API key";
                }
            }
            
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(responseMessage));
        }
    }
}
