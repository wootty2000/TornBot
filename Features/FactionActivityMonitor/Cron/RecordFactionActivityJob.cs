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
using Quartz.Xml.JobSchedulingData20;
using TornBot.Entities;
using TornBot.Exceptions;
using TornBot.Services.Cron.Infrastructure;
using TornBot.Services.Factions.Services;
using TornBot.Services.Players.Service;
using TornBot.Services.TornApi.Services;

namespace TornBot.Features.FactionActivityMonitor.Cron;

public class RecordFactionActivityJob : WorkerJob
{
    private static string _cronExpression = "0 */5 * * * ? *";

    private readonly IConfigurationRoot _config;
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
        IConfigurationRoot config,
        ILogger<RecordFactionActivityJob> logger,
        TornApiService tornApiService,
        PlayersService playersService,
        FactionsService factionsService
    )
    {
        _config = config;
        _logger = logger;
        _tornApiService = tornApiService;
        _playersService = playersService;
        _factionsService = factionsService;
    }

    public Task Execute(IJobExecutionContext context)
    {
        try
        {
            List<TornFaction> factionsList = _factionsService.GetFactionsForMonitoring();

            foreach (var factionList in factionsList)
            {
                TornBot.Entities.TornFaction factions = _tornApiService.GetFaction(factionList.Id);

                foreach (var member in factions.Members)
                {
                    _playersService.RecordPlayerStatus(member);
                    // TODO Update DB TornPlayers member name
                }
            }
        }
        catch (ApiCallFailureException e)
        {
            _logger.LogError(e, "Failed to record faction member activity");
        }


        return Task.CompletedTask;
    }
}