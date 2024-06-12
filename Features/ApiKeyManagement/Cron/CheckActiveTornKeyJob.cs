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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using TornBot.Services.Cron.Infrastructure;
using TornBot.Services.TornApi.Services;

namespace TornBot.Features.ApiKeyManagement.Cron;

public class CheckActiveTornKeyJob : WorkerJob
{
    private readonly TornApiService _tornApiService;
    
    private readonly ILogger<CheckActiveTornKeyJob> _logger;
    private static string _cronExpression = "0 5 1 * * ? *";

    public static void AddJob(IServiceCollection services)
    {
        services.AddQuartz(conf =>
        {
            JobKey jobKey = new JobKey("CheckActiveTornKeys-Job", "Cron");
            conf.AddJob<CheckActiveTornKeyJob>(j => j.WithIdentity(jobKey));
            conf.AddTrigger(trigger => trigger
                .WithIdentity("CheckActiveTornKeys-Trigger", "Cron")
                .ForJob(jobKey)
                .WithCronSchedule(_cronExpression));
        });
        services.AddSingleton<CheckActiveTornKeyJob>();
    }
    
    public CheckActiveTornKeyJob(ILogger<CheckActiveTornKeyJob> logger, TornApiService tornApiService)
    {
        _logger = logger;
        _tornApiService = tornApiService;
    }
    
    public Task Execute(IJobExecutionContext context)
    {
        try
        {
            _tornApiService.CheckAllActiveApiKeys();

        }
        catch (Exception)
        {
            // TODO - Check is there is anything we actually want / need to do. 
            // Probably just ignore and exceptions and try again on the next cron cycle 
        }
        
        return Task.CompletedTask;
    }
}