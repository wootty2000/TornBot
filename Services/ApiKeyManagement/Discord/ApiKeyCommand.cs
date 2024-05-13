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

using DSharpPlus.SlashCommands;
using TornBot.Services.Players.Service;

namespace TornBot.Services.ApiKeyManagement.Discord
{
    public class ApiKeyCommand : ApplicationCommandModule
    {
        PlayersService _players;

        public ApiKeyCommand(
            PlayersService players)
        {
            _players = players;
        }
        /*
        public class ChoiceProvider : IChoiceProvider
        {
            public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
            {
                return new DiscordApplicationCommandOptionChoice[]
                {
                new DiscordApplicationCommandOptionChoice("Torn", "0"),
                new DiscordApplicationCommandOptionChoice("TornStats", "1")
                };
            }
        }*/
        
        [SlashCommand("addapi", "Add api key")]
        public async Task ApiKey(
            InteractionContext ctx,
            //[ChoiceProvider(typeof(ChoiceProvider))]
            [Option("apikey", "Add torn api key with public access level")] string api_key)
            //[Option("TornStatsApiKey", "Add torn stats api key - not required (you can input 0)")] string TornStats_api_key)
        {
            api_key = api_key.Trim();
            string api_key_respons = "";
            
            api_key_respons = _players.AddApiKey(api_key);
            
            await ctx.CreateResponseAsync(api_key_respons, ephemeral: true);
            
            
        }

    }
}
