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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace TornBot.Services.Cron.Infrastructure;

public class HelloWorldJob : WorkerJob
{
    private readonly ILogger<HelloWorldJob> _logger;
    private static string _cronExpression = "0/5 * * * * ? *";
    
    public static void AddJob(IServiceCollection services)
    {
        services.AddQuartz(conf =>
        {
            JobKey jobKey = new JobKey("HelloWorldJob-Job", "Cron");
            conf.AddJob<HelloWorldJob>(j => j.WithIdentity(jobKey));
            conf.AddTrigger(trigger => trigger
                .WithIdentity("HelloWorldJob-Trigger", "Cron")
                .ForJob(jobKey)
                .WithCronSchedule(_cronExpression));
        });
        services.AddTransient<HelloWorldJob>();
    }
    
    public HelloWorldJob(ILogger<HelloWorldJob> logger)
    {
        _logger = logger;
    }
    
    public void Dispose() { } // No specific cleanup required here (optional)
    
    public Task Execute(IJobExecutionContext context)
    {
        //_logger.LogInformation("HelloWorldJob running");
        
        return Task.CompletedTask;
    }
}