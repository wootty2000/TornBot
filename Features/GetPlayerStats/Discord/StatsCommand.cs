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

using System.Globalization;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using TornBot.Exceptions;
using TornBot.Services.Players.Service;

namespace TornBot.Features.GetPlayerStats.Discord
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
            [Option("PlayerID", "ID of the player or name")] string idOrName)
        {
            await ctx.DeferAsync();

            idOrName = idOrName.Trim();

            Entities.BattleStats battleStats;
            try
            {
                battleStats = _players.GetBattleStats(idOrName);
            }
            catch (BattleStatsNotAvailableException e)
            {
                string message;
                if (e.InnerException is ApiCallFailureException apiException)
                {
                    message = apiException.InnerException switch
                    {
                        PlayerNotFoundException => "No Battle Stats found for player",
                        ApiNotAvailableException => "No local Battle Stats and TornStats API is unavailable. Please try again later",
                        InvalidKeyException => "No local Battle Stats and no available API keys for TornStats API. Please try again later",
                        _ => "Unknown error getting Battle Stats. Please try again later"
                    };

                }
                else
                    message = "Unknown error getting Battle Stats. Please try again later";
                
                ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
                return;
            }
            catch (Exception e)
            {
                string message = "Unknown error getting Battle Stats. Please try again later";
                
                ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
                return;
            }

            Entities.TornPlayer player;
            try
            {
                player = _players.GetPlayer(battleStats.PlayerId);
            }
            catch (ApiCallFailureException e)
            {
                string message;
                if (e.InnerException is not null)
                {
                    message = e.InnerException switch
                    {
                        ApiNotAvailableException => "Torn API is unavailable. Please try again later",
                        InvalidKeyException => "No available API keys for TornStats API. Please try again later",
                        _ => "Unknown error getting BattleStats. Please try again later"
                    };

                }
                else
                    message = "Unknown error getting BattleStats. Please try again later";
                
                ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
                return;
            }
            catch (Exception e)
            {
                string message = "Unknown error getting Battle Stats. Please try again later";
                
                ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
                return;
            }
            
            //Build a response for the user
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            embed.Title = String.Format("{0} [{1}]", player.Name, player.Id);
            embed.Description = "Lvl: " + player.Level + "\n" + "Faction: " + player.Faction.Name;

            embed.AddField("Battle Stats:",
                "Strength: " + battleStats.Strength.ToString("N0", CultureInfo.InvariantCulture) + "\n" +
                "Speed: " + battleStats.Speed.ToString("N0", CultureInfo.InvariantCulture) + "\n" +
                "Defense: " + battleStats.Defense.ToString("N0", CultureInfo.InvariantCulture) + "\n" +
                "Dexterity: " + battleStats.Dexterity.ToString("N0", CultureInfo.InvariantCulture) + "\n" +
                "Total: " + battleStats.Total.ToString("N0", CultureInfo.InvariantCulture) + "\n" +
                $"[Profile](https://www.torn.com/profiles.php?XID={0})     [Attack](https://www.torn.com/loader.php?sid=attack&user2ID={0})")//between -> "" is "non-breaking space"
             .WithFooter(battleStats.BattleStatsTimestamp.ToString("yyyy-MM-dd HH:mm"));


            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }
    }
}