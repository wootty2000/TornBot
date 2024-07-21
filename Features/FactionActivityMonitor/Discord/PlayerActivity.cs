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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using TornBot.Entities;
using TornBot.Services.Discord.Interfaces;
using TornBot.Services.Players.Database.Dao;
using TornBot.Services.Players.Service;

namespace TornBot.Features.FactionActivityMonitor.Discord;

public class PlayerActivity : ApplicationCommandModule, IDiscordEventHandlerModule
{
    private PlayersService _playersService;
    private IPlayerStatusDao _playerStatusDao;
    private IPlayerActivityImageService _imageService;
    
    public PlayerActivity(
        PlayersService playersService, 
        IPlayerStatusDao playerStatusDao,
        IPlayerActivityImageService imageService
    )
    {
        _playersService = playersService;
        _playerStatusDao = playerStatusDao;
        _imageService = imageService;
    }

    public void RegisterEventHandlers(DiscordClient client)
    {
        client.ComponentInteractionCreated += ComponentEventHandler;
    }

    [SlashCommand("PlayerActivity", "Gets the activity of a player")]
    public async Task GetPlayerActivity(
        InteractionContext ctx,
        [Option("PlayerId", "Player Id")] long id
    )
    {
        await ctx.DeferAsync();

        RespondWithYears(ctx, (UInt32)id);
    }
    
    private Task ComponentEventHandler(DiscordClient sender, ComponentInteractionCreateEventArgs args)
    {
        if (
            args.Interaction.Type == InteractionType.Component && 
            args.Interaction.Data.CustomId.StartsWith("PlayerActivity")
        )
        {
            string data = args.Interaction.Data.CustomId;
            
            switch (data.Count(c => c == '_'))
            {
                case 2: // PLayerActivity_1234_2024
                    RespondWithMonths(args);
                    break;
                case 3: // PlayerAcivity_1234_2024_04
                    RespondWithDays(args);
                    break;
                case 4: // PlayerAcivitiy_1234_2024_04_01
                    RespondWithGraph(args);
                    break;
            }
        }
        
        return Task.CompletedTask;
    }

    
    private async void RespondWithYears(InteractionContext ctx, UInt32 playerId)
    {
        List<DateTime> dates = _playersService.GetPlayerStatusDatesForPlayer(playerId);
        
        List<int> years = dates.Select(date => date.Year).Distinct().OrderBy(year => year).ToList();
        
        List<DiscordComponent> components = new List<DiscordComponent>();
        DiscordWebhookBuilder builder = new DiscordWebhookBuilder()
            .WithContent($"Activity for Player {playerId}\nSelect the year");
        foreach (var year in years)
        {
            components.Add(new DiscordButtonComponent(ButtonStyle.Primary, $"PlayerActivity_{playerId}_{year}", year.ToString()));
            
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
        string playerId = data[1];
        string year = data[2];
        
        List<DateTime> dates = _playersService.GetPlayerStatusDatesForPlayer(Convert.ToUInt32(playerId));
        List<int> months = dates
            .Where(date => date.Year == Convert.ToInt16(year))
            .Select(date => date.Month)
            .Distinct()
            .OrderBy(month => month)
            .ToList();
        
        DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
            .WithContent($"Activity for Player {playerId}\nYear:{year}\nSelect the month");
        List<DiscordComponent> components = new List<DiscordComponent>();

        foreach (var month in months)
        {
            components.Add(new DiscordButtonComponent(ButtonStyle.Primary, $"PlayerActivity_{playerId}_{year}_{month:D2}", month.ToString()));
            
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
        string playerId = data[1];
        string year = data[2];
        string month = data[3];
        
        List<DateTime> dates = _playersService.GetPlayerStatusDatesForPlayer(Convert.ToUInt32(playerId));
        List<int> days = dates
            .Where(date => date.Year == Convert.ToInt16(year) && date.Month == Convert.ToInt16(month))
            .Select(date => date.Day)
            .Distinct()
            .OrderBy(day => day)
            .ToList();
        
        DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
            .WithContent($"Activity for Player {playerId}\nYear:{year} Month:{month}\nSelect the day for week commencing");
        List<DiscordComponent> components = new List<DiscordComponent>();

        foreach (var day in days)
        {
            components.Add(new DiscordButtonComponent(ButtonStyle.Primary, $"PlayerActivity_{playerId}_{year}_{month:D2}_{day:D2}", day.ToString()));
            
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
        string playerId = data[1];
        string year = data[2];
        string month = data[3];
        string day = data[4];

        try
        {
            var webhookBuilder = new DiscordInteractionResponseBuilder();
            
            PlayerStatusData playerStatus = _playersService.GetPlayerStatusData(Convert.ToUInt32(playerId), DateTime.Parse($"{year}-{month}-{day}"));
            if (playerStatus.Id == 0)
            {
                webhookBuilder
                    .WithContent("No data found");
            }
            else
            {
                Stream imageStream = _imageService.GeneratePlayerActivityImage(playerStatus);

                webhookBuilder
                    .AddFile("player_activity.png", imageStream)
                    .WithContent("Player activity for the specified player:");

            }
            
            await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, webhookBuilder);
        }
        catch (Exception ex)
        {
            // Handle any exceptions or log errors
            Console.WriteLine($"Error processing slash command: {ex.Message}");
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent("Error getting data or building player activity image")
            );
        }   
    }
}
