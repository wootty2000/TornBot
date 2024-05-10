﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using TornBot.Services.Players.Service;
using TornBot.Services.TornApi.Services;
using TornBot.Services.TornStatsApi.Services;

namespace TornBot.Services.Discord.Commands.Slash
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