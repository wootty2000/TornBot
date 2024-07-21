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
using TornBot.Entities;
using TornBot.Services.Factions.Services;

namespace TornBot.Features.ReviveMonitor.Discord
{
    public class RevivesCommand : ApplicationCommandModule
    {
        private FactionsService _factionsService;
        
        public RevivesCommand(FactionsService factionsService)
        {
            _factionsService = factionsService;
        }
        
        [SlashCommand("Revives", "Gets the revive status of a players from the faction")]
        public async Task Stats(
            InteractionContext ctx,
            [Option("Faction_ID", "ID of the faction")] long id)
        {
            await ctx.DeferAsync();

            //return list of players that can be revived
            //TODO Wrap in Try/catch block as GetReviveStatus can throw an exception
            //Torn API Error 6 is Wrong Id. this needs checking
            List<Entities.TornPlayer> tornPlayerList = _factionsService.GetReviveStatus((UInt32)id, out bool usedInsideKey, ref ctx);

            if (tornPlayerList.Count == 0)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No revivable players"));
                return;
            }

            string allRevivablePlayers = string.Join("\n", tornPlayerList.Select(tornPlayer => $"[{tornPlayer.Name}](https://www.torn.com/profiles.php?XID={tornPlayer.Id})"));

            TornFaction faction = _factionsService.GetFaction(tornPlayerList.First().FactionId);
            string factionName = faction.Name;
            UInt32 factionID = faction.Id;
            string faction_tag = faction.Tag_image;
            
            string embedTitle = "Revivable players" + (usedInsideKey ? "\nWARNING - Inside key user. Expect false positives:" : ":");

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
                Title = embedTitle,
                Author = embedAuthor,
                Description = allRevivablePlayers,
                Footer = embedFooter,
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

    }
}
