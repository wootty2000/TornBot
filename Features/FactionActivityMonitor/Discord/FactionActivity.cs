// TornBot
// 
// Copyright (C) 2024 TornBot.com
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
// 
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.ComponentModel.Design;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using TornBot.Entities;
using TornBot.Services.Discord.Interfaces;
using TornBot.Services.Factions.Services;
using TornBot.Services.Players.Database.Dao;
using TornBot.Services.Players.Database.Entities;
using TornBot.Services.Players.Service;
using TornPlayer = TornBot.Entities.TornPlayer;

namespace TornBot.Features.FactionActivityMonitor.Discord;

public class FactionActivity : ApplicationCommandModule, IDiscordEventHandlerModule
{
    private PlayersService _playersService;
    private FactionsService _factionsService;
    private IPlayerStatusDao _playerStatusDao;
    private IPlayerActivityImageService _imageService;
    
    public FactionActivity(
        
        PlayersService playersService, 
        FactionsService factionsService,
        IPlayerStatusDao playerStatusDao,
        IPlayerActivityImageService imageService
    )
    {
        _playersService = playersService;
        _factionsService = factionsService;
        _playerStatusDao = playerStatusDao;
        _imageService = imageService;
    }

    public void RegisterEventHandlers(DiscordClient client)
    {
        client.ComponentInteractionCreated += ComponentEventHandler;
    }
    
    private Task ComponentEventHandler(DiscordClient sender, ComponentInteractionCreateEventArgs args)
    {
        if (
            args.Interaction.Type == InteractionType.Component && 
            args.Interaction.Data.CustomId.StartsWith("FactionActivity")
        )
        {
            string data = args.Interaction.Data.CustomId;
            
            switch (data.Count(c => c == '_'))
            {
                case 2: // FactionActivity_1234_2024
                    RespondWithMonths(args);
                    break;
                case 3: // FactionActivity_1234_2024_04
                    RespondWithDays(args);
                    break;
                case 4: // FactionActivity_1234_2024_04_01
                    RespondWithGraph(args);
                    break;
            }
        }
        
        return Task.CompletedTask;
    }
    
    [SlashCommand("FactionActivity", "Gets the faction heatmap")]
    public async Task GetFactionActivity(
        InteractionContext ctx,
        [Option("FactionId", "Faction Id")] long id)
    {
        await ctx.DeferAsync();

        RespondWithYears(ctx, (UInt32)id);
    }

    private async void RespondWithYears(InteractionContext ctx, UInt32 factionId)
    {
        List<TornPlayer> players = _playersService.GetPlayersInFaction(Convert.ToUInt32(factionId));

        List<DateTime> dates = new List<DateTime>();
        foreach (var player in players)
        {
            List<DateTime> playerDates = _playersService.GetPlayerStatusDatesForPlayer(Convert.ToUInt32(player.Id));
            foreach (var playerDate in playerDates)
            {
                if(!dates.Contains(playerDate))
                    dates.Add(playerDate);
            }
        }
        
        List<int> years = dates.Select(date => date.Year).Distinct().OrderBy(year => year).ToList();
        
        List<DiscordComponent> components = new List<DiscordComponent>();
        DiscordWebhookBuilder builder = new DiscordWebhookBuilder()
            .WithContent($"Faction heatmap for Faction {factionId}\nSelect the year");
        foreach (var year in years)
        {
            components.Add(new DiscordButtonComponent(ButtonStyle.Primary, $"FactionActivity_{factionId}_{year}", year.ToString()));
            
            if (components.Count == 5)
            {
                builder.AddComponents(components.ToArray());
                components.Clear();
            }
        }

        if (components.Count > 0)
        {
            builder.AddComponents(components.ToArray());
        }

        await ctx.EditResponseAsync(builder);
    }

    private async void RespondWithMonths(ComponentInteractionCreateEventArgs args)
    {
        string[] data = args.Interaction.Data.CustomId.Split('_');
        string factionId = data[1];
        string year = data[2];
        
        List<TornPlayer> players = _playersService.GetPlayersInFaction(Convert.ToUInt32(factionId));

        List<DateTime> dates = new List<DateTime>();
        foreach (var player in players)
        {
            List<DateTime> playerDates = _playersService.GetPlayerStatusDatesForPlayer(Convert.ToUInt32(player.Id));
            foreach (var playerDate in playerDates)
            {
                if(!dates.Contains(playerDate))
                    dates.Add(playerDate);
            }
        }

        List<int> months = dates
            .Where(date => date.Year == Convert.ToInt16(year))
            .Select(date => date.Month)
            .Distinct()
            .OrderBy(month => month)
            .ToList();
        
        DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
            .WithContent($"Faction heatmap for Faction {factionId}\nYear:{year}\nSelect the month");
        List<DiscordComponent> components = new List<DiscordComponent>();

        foreach (var month in months)
        {
            components.Add(new DiscordButtonComponent(ButtonStyle.Primary, $"FactionActivity_{factionId}_{year}_{month:D2}", month.ToString()));
            
            if (components.Count == 5)
            {
                builder.AddComponents(components.ToArray());
                components.Clear();
            }
        }

        if (components.Count > 0)
        {
            builder.AddComponents(components.ToArray());
        }

        await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, builder);
    }
    
    private async void RespondWithDays(ComponentInteractionCreateEventArgs args)
    {
        string[] data = args.Interaction.Data.CustomId.Split('_');
        string factionId = data[1];
        string year = data[2];
        string month = data[3];
        
        List<TornPlayer> players = _playersService.GetPlayersInFaction(Convert.ToUInt32(factionId));

        List<DateTime> dates = new List<DateTime>();
        foreach (var player in players)
        {
            List<DateTime> playerDates = _playersService.GetPlayerStatusDatesForPlayer(Convert.ToUInt32(player.Id));
            foreach (var playerDate in playerDates)
            {
                if(!dates.Contains(playerDate))
                    dates.Add(playerDate);
            }
        }
        
        List<int> days = dates
            .Where(date => date.Year == Convert.ToInt16(year) && date.Month == Convert.ToInt16(month))
            .Select(date => date.Day)
            .Distinct()
            .OrderBy(day => day)
            .ToList();
        
        DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
            .WithContent($"Faction heatmap for Faction {factionId}\nYear:{year} Month:{month}\nSelect the day for week commencing");
        List<DiscordComponent> components = new List<DiscordComponent>();

        foreach (var day in days)
        {
            components.Add(new DiscordButtonComponent(ButtonStyle.Primary, $"FactionActivity_{factionId}_{year}_{month:D2}_{day:D2}", day.ToString()));
            
            if (components.Count == 5)
            {
                builder.AddComponents(components.ToArray());
                components.Clear();
            }
        }

        if (components.Count > 0)
        {
            builder.AddComponents(components.ToArray());
        }

        await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, builder);
    }
    
    private async void RespondWithGraph(ComponentInteractionCreateEventArgs args)
    {
        string[] data = args.Interaction.Data.CustomId.Split('_');
        UInt32 factionId = Convert.ToUInt32(data[1]);
        string year = data[2];
        string month = data[3];
        string day = data[4];
        DateTime startDate = DateTime.Parse($"{year}-{month}-{day}");
        
        try
        {
            var webhookBuilder = new DiscordInteractionResponseBuilder();
            
            TornFaction faction = _factionsService.GetFaction(factionId);
            if (faction.Id == 0)
            {
                webhookBuilder.WithContent("Can not find faction by id " + factionId);

                await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, webhookBuilder);
                return;
            }
            
            List<TornPlayer> players = _playersService.GetPlayersInFaction(factionId);

            List<PlayerStatusData> statuses = new List<PlayerStatusData>();
            foreach (var player in players)
            {
                PlayerStatusData status = _playersService.GetPlayerStatusData(Convert.ToUInt32(player.Id), startDate);
                
                if(status.Id > 0)
                    statuses.Add(status);
            }
            
            Stream imageStream = _imageService.GenerateFactionHeatmapImage(statuses, faction, startDate);
            
            // Send the final response with the image
            webhookBuilder
                .AddFile($"faction_heatmap-{factionId}-{year}-{month}-{day}.png", imageStream)
                .WithContent("Faction heatmap:");

            await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, webhookBuilder);
        }
        catch (Exception ex)
        {
            // Handle any exceptions or log errors
            Console.WriteLine($"Error processing slash command: {ex.Message}");
            //await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            //    new DiscordInteractionResponseBuilder()
            //        .WithContent("An error occurred while processing the command.")
            //);
        }   
    }
}