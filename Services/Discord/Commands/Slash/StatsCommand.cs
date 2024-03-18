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
using DSharpPlus.SlashCommands;
using System.Globalization;
using TornBot.Services.TornApi.Services;
using TornBot.Services.TornStatsApi.Services;

namespace TornBot.Services.Discord.Commands.Slash
{
    public class StatsCommand : ApplicationCommandModule
    {
        TornApiService tornApiService;
        TornStatsApiService tornstatsApi;

        public StatsCommand(
            TornApiService tornAPIService,
            TornStatsApiService tornstatsApi)
        {
            this.tornApiService = tornAPIService;
            this.tornstatsApi = tornstatsApi;
        }

        [SlashCommand("Stats", "Gets the stats of a player")]
        public async Task Stats(
            InteractionContext ctx,
            [Option("PlayerID", "ID of the player")] long id)
        {
            await ctx.DeferAsync();

            Entities.TornPlayer player = tornApiService.GetPlayer((UInt32)id);
            Entities.Stats stats = tornstatsApi.GetStats((UInt32)id);
            
            //If either of the objects is null, then tell the user there was an error
            if(player == null || stats == null)
            {
                ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error getting stats"));

                return;
            }

            //Build a response for the user
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            embed.Title = String.Format("{0} [{1}]", player.Name, id);

            embed.AddField("Stats:",
                "Strength: " + stats.Strength.ToString("N0", CultureInfo.InvariantCulture) + "\n" +
                "Speed: " + stats.Speed.ToString("N0", CultureInfo.InvariantCulture) + "\n" +
                "Defense: " + stats.Defense.ToString("N0", CultureInfo.InvariantCulture) + "\n" +
                "Dexterity: " + stats.Dexterity.ToString("N0", CultureInfo.InvariantCulture) + "\n" +
                "Total: " + stats.Total.ToString("N0", CultureInfo.InvariantCulture) + "\n" +
                $"[Profile](https://www.torn.com/profiles.php?XID={0})     [Attack](https://www.torn.com/loader.php?sid=attack&user2ID={0})")//between -> "" is "non-breaking space"
             .WithFooter(stats.StatsTimestamp.ToString("yyyy-MM-dd HH:mm"));


            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }
    }
}