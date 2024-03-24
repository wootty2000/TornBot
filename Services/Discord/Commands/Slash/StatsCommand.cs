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
using System.Globalization;
using TornBot.Services.Players.Service;
using TornBot.Services.TornApi.Services;
using TornBot.Services.TornStatsApi.Services;

namespace TornBot.Services.Discord.Commands.Slash
{
    public class StatsCommand : ApplicationCommandModule
    {
        PlayersService _players;

        public StatsCommand(
            PlayersService players)
        {
            _players = players;
        }

        [SlashCommand("Stats", "Gets the stats of a player")]
        public async Task Stats(
            InteractionContext ctx,
            [Option("PlayerID", "ID of the player or name")] string id)
        {
            await ctx.DeferAsync();

            //id = id.Replace(" ", ""); //this is to make sure there is no space before id/name
            id = id.Trim();

            Entities.Stats stats = _players.GetStats(id);


            if (stats == null)
            {
                ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error getting stats from TornStats"));
                return;
            }

            Entities.TornPlayer player = _players.GetPlayer(stats.PlayerId);

            if (player == null)
            {
                ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error getting stats from Torn"));
                return;
            }


            //Build a response for the user
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            embed.Title = String.Format("{0} [{1}]", player.Name, player.Id);
            embed.Description = "Lvl: " + player.Level + "\n" + "Faction: " + player.Faction.Name;

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