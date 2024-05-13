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
using TornBot.Services.Players.Service;

namespace TornBot.Features.ReviveMonitor.Discord
{
    public class RevivesCommand : ApplicationCommandModule
    {
        PlayersService _players;
        
        public RevivesCommand(
            PlayersService players)
        {
            _players = players;
        }
        
        [SlashCommand("Revives", "Gets the revive status of a players from the faction")]
        public async Task Stats(
            InteractionContext ctx,
            [Option("Faction_ID", "ID of the faction")] long id)
        {
            await ctx.DeferAsync();

            //return list of players that can be revived
            List<Entities.ReviveStatus> reviveStatus = _players.GetReviveStatus((UInt32)id);

            if (reviveStatus.Count == 0)
            {
                ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No revivable players"));
                return;
            }

            string allRevivablePlayers = string.Join("\n", reviveStatus.Select(rs => $"[{rs.Player.Name}](https://www.torn.com/profiles.php?XID={rs.Player.Id})"));

            string factionName = reviveStatus.FirstOrDefault().Player.Faction.Name;
            UInt32 factionID = reviveStatus.FirstOrDefault().Player.Faction.Id;
            string faction_tag = reviveStatus.FirstOrDefault().Player.Faction.Tag_image;
            
            DateTime timeNow = DateTime.Now;

            DiscordEmbedBuilder.EmbedAuthor embedAuthor = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = factionName,
                IconUrl = $"https://factiontags.torn.com/{faction_tag}",
                Url = $"https://www.torn.com/factions.php?step=profile&ID={factionID}"
            };
            DiscordEmbedBuilder.EmbedFooter embedFooter = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            };
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Revivable players:",
                Author = embedAuthor,
                Description = allRevivablePlayers,
                Footer = embedFooter,
            };
            

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

    }
}
