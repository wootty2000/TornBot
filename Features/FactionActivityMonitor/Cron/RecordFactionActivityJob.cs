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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using TornBot.Entities;
using TornBot.Exceptions;
using TornBot.Services.Cron.Infrastructure;
using TornBot.Services.Factions.Services;
using TornBot.Services.Players.Service;
using TornBot.Services.TornApi.Services;

namespace TornBot.Features.FactionActivityMonitor.Cron;

public class RecordFactionActivityJob : WorkerJob
{
    private readonly int _maxNumberThreads = 10;
    private static string _cronExpression = "0 */5 * * * ? *";

    private readonly ILogger<RecordFactionActivityJob> _logger;
    private readonly TornApiService _tornApiService;
    private readonly PlayersService _playersService;
    private readonly FactionsService _factionsService;
    
    public static void AddJob(IServiceCollection services)
    {
        services.AddQuartz(conf =>
        {
            JobKey jobKey = new JobKey("RecordFactionActivityJob-Job", "Cron");
            conf.AddJob<RecordFactionActivityJob>(j => j.WithIdentity(jobKey));
            conf.AddTrigger(trigger => trigger
                .WithIdentity("RecordFactionActivityJob-Trigger", "Cron")
                .ForJob(jobKey)
                .WithCronSchedule(_cronExpression));
        });
        services.AddScoped<RecordFactionActivityJob>();
    }
    
    public RecordFactionActivityJob(
        ILogger<RecordFactionActivityJob> logger,
        TornApiService tornApiService,
        PlayersService playersService,
        FactionsService factionsService
    )
    {
        _logger = logger;
        _tornApiService = tornApiService;
        _playersService = playersService;
        _factionsService = factionsService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            List<TornFaction> dbFactionsList = _factionsService.GetFactionsForMonitoring();
            List<TornFaction> factionsList = new List<TornFaction>();
            DateTime now = DateTime.UtcNow;
            
            var semaphore = new SemaphoreSlim(_maxNumberThreads);

            var factionTasks = dbFactionsList.Select(async dbFaction =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await Task.Run(() =>
                    {
                        TornBot.Entities.TornFaction faction = _tornApiService.GetFaction(dbFaction.Id);
                        factionsList.Add(faction);
                    });
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();
            await Task.WhenAll(factionTasks);

            foreach (var faction in factionsList)
            {
                foreach (var member in faction.Members)
                {
                    _playersService.RecordPlayerStatus(member, now);
                    _playersService.SavePlayer(member);
                }

                _factionsService.UpdateFaction(faction);
            }
        }
        catch (ApiCallFailureException e)
        {
            _logger.LogError(e, "Failed to record faction activity");
        }
    }
}